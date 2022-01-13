﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DotNetEd.CoreAdmin.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetEd.CoreAdmin.Controllers
{
    [CoreAdminAuth]
    public class CoreAdminDataController : Controller
    {
        private readonly IEnumerable<DiscoveredDbSetEntityType> dbSetEntities;

        public CoreAdminDataController(IEnumerable<DiscoveredDbSetEntityType> dbSetEntities)
        {
            this.dbSetEntities = dbSetEntities;
        }


        [HttpGet]
        public IActionResult Index(string id)
        {
            var viewModel = new DataListViewModel();

            foreach (var dbSetEntity in dbSetEntities.Where(db => db.Name.ToLowerInvariant() == id.ToLowerInvariant()))
            {
                foreach (var dbSetProperty in dbSetEntity.DbContextType.GetProperties())
                {
                    if (dbSetProperty.PropertyType.IsGenericType && dbSetProperty.PropertyType.Name.StartsWith("DbSet") && dbSetProperty.Name.ToLowerInvariant() == id.ToLowerInvariant())
                    {
                        viewModel.EntityType = dbSetProperty.PropertyType.GetGenericArguments().First();
                        viewModel.DbSetProperty = dbSetProperty;

                        var dbContextObject = (DbContext)this.HttpContext.RequestServices.GetRequiredService(dbSetEntity.DbContextType);

                        var dbSetValue = dbSetProperty.GetValue(dbContextObject);

                        viewModel.Data = (IEnumerable<object>)dbSetValue;
                        viewModel.DbContext = dbContextObject;
                    }
                }
            }

            if (viewModel.DbContext == null)
            {
                return NotFound();
            }

            return View(viewModel);
        }

        private object GetDbSetValueOrNull(string dbSetName, out DbContext dbContextObject, out Type typeOfEntity)
        {
            foreach (var dbSetEntity in dbSetEntities.Where(db => db.Name.ToLowerInvariant() == dbSetName.ToLowerInvariant()))
            {
                foreach (var dbSetProperty in dbSetEntity.DbContextType.GetProperties())
                {
                    if (dbSetProperty.PropertyType.IsGenericType && dbSetProperty.PropertyType.Name.StartsWith("DbSet") && dbSetProperty.Name.ToLowerInvariant() == dbSetName.ToLowerInvariant())
                    {
                        dbContextObject = (DbContext)this.HttpContext.RequestServices.GetRequiredService(dbSetEntity.DbContextType);
                        typeOfEntity = dbSetProperty.PropertyType.GetGenericArguments()[0];
                        return dbSetProperty.GetValue(dbContextObject);
                    }
                }
            }

            dbContextObject = null;
            typeOfEntity = null;
            return null;
        }

        private object GetEntityFromDbSet(string dbSetName, string id, out DbContext dbContextObject, out Type typeOfEntity)
        {
            var dbSetValue = GetDbSetValueOrNull(dbSetName, out dbContextObject, out typeOfEntity);

            var primaryKey = dbContextObject.Model.FindEntityType(typeOfEntity).FindPrimaryKey();
            var clrType = primaryKey.Properties[0].ClrType;

            object convertedPrimaryKey = id;
            if (clrType == typeof(Guid))
            {
                convertedPrimaryKey = Guid.Parse(id);
            }
            else if (clrType == typeof(int))
            {
                convertedPrimaryKey = int.Parse(id);
            }
            else if (clrType == typeof(Int64))
            {
                convertedPrimaryKey = Int64.Parse(id);
            }

            return dbSetValue.GetType().InvokeMember("Find", BindingFlags.InvokeMethod, null, dbSetValue, args: new object[] { convertedPrimaryKey });

        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> CreateEntityPost(string dbSetName, string id, [FromForm] object formData)
        {
            var dbSetValue = GetDbSetValueOrNull(dbSetName, out var dbContextObject, out var entityType);

            var newEntity = Activator.CreateInstance(entityType);

            var autoGeneratedPropertyNames =
            newEntity.GetType().GetProperties()
            .Where(p => p.GetCustomAttributes().Any(a => a.GetType().Name.Contains("DatabaseGenerated"))).Select(p => p.Name);

            if (await TryUpdateModelAsync(newEntity, entityType, string.Empty, 
                await CompositeValueProvider.CreateAsync(this.ControllerContext, this.ControllerContext.ValueProviderFactories),
                (ModelMetadata meta) => !autoGeneratedPropertyNames.Contains(meta.PropertyName)))
            {
                if (TryValidateModel(newEntity))
                {
                    // updated model with new values
                    dbContextObject.Add(newEntity);
                    await dbContextObject.SaveChangesAsync();
                    return RedirectToAction("Index", new {id = dbSetName });
                }
            } 

            ViewBag.DbSetName = id;

            ViewBag.IgnoreFromForm = autoGeneratedPropertyNames;

            return View("Create", newEntity);
        }

        [HttpGet]
        [IgnoreAntiforgeryToken]
        public IActionResult Create(string id)
        {
            var dbSetValue = GetDbSetValueOrNull(id, out var dbContextObject, out var entityType);

            var newEntity = Activator.CreateInstance(entityType);
            ViewBag.DbSetName = id;

            var autoGeneratedPropertyNames =
                newEntity.GetType().GetProperties()
                .Where(p => p.GetCustomAttributes().Any(a => a.GetType().Name.Contains("DatabaseGenerated"))).Select(p => p.Name);

            ViewBag.IgnoreFromForm = autoGeneratedPropertyNames;
            return View(newEntity);
        }

        [HttpGet]
        public IActionResult EditEntity(string dbSetName, string id)
        {
            var entityToEdit = GetEntityFromDbSet(dbSetName, id, out var dbContextObject, out var entityType);

            ViewBag.DbSetName = dbSetName;
            ViewBag.Id = id;
            return View("Edit", entityToEdit);
        }



        [HttpPost]
        public async Task<IActionResult> EditEntityPost(string dbSetName, string id, [FromForm] object formData)
        {
            var entityToEdit = GetEntityFromDbSet(dbSetName, id, out var dbContextObject, out var entityType);

            dbContextObject.Attach(entityToEdit);

            if (await TryUpdateModelAsync(entityToEdit, entityType, string.Empty))
            {
                if (TryValidateModel(entityToEdit))
                {
                    await dbContextObject.SaveChangesAsync();
                    return RedirectToAction("Index", new {id = dbSetName});
                }
            }

            ViewBag.DbSetName = dbSetName;
            ViewBag.Id = id;

            return View("Edit", entityToEdit);
        }

        [HttpGet]
        public IActionResult DeleteEntity(string dbSetName, string id)
        {
            var viewModel = new DataDeleteViewModel();
            viewModel.DbSetName = dbSetName;
            viewModel.Id = id;
            viewModel.Object = GetEntityFromDbSet(dbSetName, id, out var dbContext, out var entityType);
            if (viewModel.Object == null) return NotFound();

            return View(viewModel);
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> DeleteEntityPost([FromForm] DataDeleteViewModel viewModel)
        {
            foreach (var dbSetEntity in dbSetEntities.Where(db => db.Name.ToLowerInvariant() == viewModel.DbSetName.ToLowerInvariant()))
            {
                foreach (var dbSetProperty in dbSetEntity.DbContextType.GetProperties())
                {
                    if (dbSetProperty.PropertyType.IsGenericType && dbSetProperty.PropertyType.Name.StartsWith("DbSet") && dbSetProperty.Name.ToLowerInvariant() == viewModel.DbSetName.ToLowerInvariant())
                    {
                        var dbContextObject = (DbContext)this.HttpContext.RequestServices.GetRequiredService(dbSetEntity.DbContextType);
                        var dbSetValue = dbSetProperty.GetValue(dbContextObject);

                        var primaryKey = dbContextObject.Model.FindEntityType(dbSetProperty.PropertyType.GetGenericArguments()[0]).FindPrimaryKey();
                        var clrType = primaryKey.Properties[0].ClrType;

                        object convertedPrimaryKey = viewModel.Id;
                        if (clrType == typeof(Guid))
                        {
                            convertedPrimaryKey = Guid.Parse(viewModel.Id);
                        } 
                        else if(clrType == typeof(int))
                        {
                            convertedPrimaryKey = int.Parse(viewModel.Id);
                        } 
                        else if (clrType == typeof(Int64))
                        {
                            convertedPrimaryKey = Int64.Parse(viewModel.Id);
                        }

                        var entityToDelete = dbSetValue.GetType().InvokeMember("Find", BindingFlags.InvokeMethod, null, dbSetValue, args: new object[] { convertedPrimaryKey });
                        dbSetValue.GetType().InvokeMember("Remove", BindingFlags.InvokeMethod, null, dbSetValue, args: new object[] {entityToDelete});

                        await dbContextObject.SaveChangesAsync();
                        
                    }
                }
            }

            return RedirectToAction("Index", new { Id = viewModel.DbSetName});

        }
    }
}
