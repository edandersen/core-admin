using DotNetEd.CoreAdmin.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

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
                        var query = dbContextObject.Set(viewModel.EntityType);

                        var dbSetValue = dbSetProperty.GetValue(dbContextObject);

                        var navProperties = dbContextObject.Model.FindEntityType(viewModel.EntityType).GetNavigations();
                        foreach (var property in navProperties)
                        {
                            // Only display One to One relationships on the Grid
                            if(property.GetCollectionAccessor() == null)    
                                query = query.Include(property.Name);
                        }

                        viewModel.Data = (IEnumerable<object>)query;
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "EF1001:Internal EF Core API usage.", Justification = "<Pending>")]
        private object GetDbSetValueOrNull(string dbSetName, out DbContext dbContextObject,
            out Type typeOfEntity,
            out Dictionary<string, Dictionary<object, string>> relationships)
        {
            foreach (var dbSetEntity in dbSetEntities.Where(db => db.Name.ToLowerInvariant() == dbSetName.ToLowerInvariant()))
            {
                foreach (var dbSetProperty in dbSetEntity.DbContextType.GetProperties())
                {
                    if (dbSetProperty.PropertyType.IsGenericType && dbSetProperty.PropertyType.Name.StartsWith("DbSet") && dbSetProperty.Name.ToLowerInvariant() == dbSetName.ToLowerInvariant())
                    {
                        dbContextObject = (DbContext)this.HttpContext.RequestServices.GetRequiredService(dbSetEntity.DbContextType);
                        typeOfEntity = dbSetProperty.PropertyType.GetGenericArguments()[0];


                        var fks = dbContextObject.Model.FindEntityType(typeOfEntity)
                            .GetForeignKeyProperties().Cast<Microsoft.EntityFrameworkCore.Metadata.RuntimeProperty>();

                        var relationshipDictionary = new Dictionary<string, Dictionary<object, string>>();
                        foreach (var f in fks)
                        {
                            var childValues = new Dictionary<object, string>();

                            if (f.ForeignKeys.Count == 1)
                            {
                                var typeOfChild = f.ForeignKeys[0];

                                var propsOnDbContext = dbContextObject.GetType().GetProperties();

                                var targetChildListOnDbContext = dbContextObject.GetType().GetProperties()
                                    .FirstOrDefault(p => p.PropertyType.IsGenericType
                                    && p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>)
                                    && p.PropertyType.GetGenericArguments().First().FullName == typeOfChild.PrincipalEntityType.Name);

                                var primaryKey2 = dbContextObject.Model.FindEntityType(typeOfChild.PrincipalEntityType.Name).FindPrimaryKey();

                                var allChildren2 = (IEnumerable<object>)dbContextObject.GetType().GetProperty(targetChildListOnDbContext.Name).GetValue(dbContextObject);

                                NullabilityInfoContext _nullabilityContext = new NullabilityInfoContext();
                                var nullabilityInfo = _nullabilityContext.Create(typeOfEntity.GetProperty(f.Name));
                                if (nullabilityInfo.WriteState == NullabilityState.Nullable)
                                {
                                    childValues.Add(string.Empty, String.Empty);
                                }

                                foreach (var childValue in allChildren2)
                                {
                                    var childPkValue = childValue.GetType().GetProperty(primaryKey2.Properties.First().Name).GetValue(childValue);
                                    childValues.Add(childPkValue, childValue.ToString());
                                }
                            }


                            relationshipDictionary.Add(f.Name, childValues);
                        }

                        relationships = relationshipDictionary;

                        return dbSetProperty.GetValue(dbContextObject);
                    }
                }
            }

            dbContextObject = null;
            typeOfEntity = null;
            relationships = null;
            return null;
        }

        private object GetEntityFromDbSet(string dbSetName, string id, 
            out DbContext dbContextObject, out Type typeOfEntity,
            out Dictionary<string, Dictionary<object, string>> relationships)
        {
            var dbSetValue = GetDbSetValueOrNull(dbSetName, out dbContextObject, out typeOfEntity, out relationships);

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
            else if (clrType == typeof(long))
            {
                convertedPrimaryKey = long.Parse(id);
            }

            return dbSetValue.GetType().InvokeMember("Find", BindingFlags.InvokeMethod, null, dbSetValue, args: new object[] { convertedPrimaryKey });

        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> CreateEntityPost(string dbSetName, string id, [FromForm] object formData)
        {
            var dbSetValue = GetDbSetValueOrNull(dbSetName, out var dbContextObject, out var entityType, out var relationships);

            var newEntity = System.Activator.CreateInstance(entityType);

            var databaseGeneratedProperties =
            newEntity.GetType().GetProperties()
            .Where(p => p.GetCustomAttributes().Any(a => a.GetType().Name.Contains("DatabaseGenerated"))).Select(p => p.Name);

            await AddByteArrayFiles(newEntity);

            await TryUpdateModelAsync(newEntity, entityType, string.Empty,
                await CompositeValueProvider.CreateAsync(this.ControllerContext, this.ControllerContext.ValueProviderFactories),
                (ModelMetadata meta) => !databaseGeneratedProperties.Contains(meta.PropertyName));

            // remove any errors from fk properties - ef will handle this validation
            foreach (var fkProperty in newEntity.GetType().GetProperties()
            .Where(p => p.GetCustomAttributes().Any(a => a.GetType().Name.Contains("ForeignKey"))).Select(p => p.Name))
            {
                if (ModelState.ContainsKey(fkProperty))
                {
                    ModelState[fkProperty].Errors.Clear();
                    ModelState[fkProperty].ValidationState = ModelValidationState.Skipped;
                }
            }

            if (ModelState.ValidationState == ModelValidationState.Valid)
            {
                // updated model with new values
                dbContextObject.Add(newEntity);
                await dbContextObject.SaveChangesAsync();
                return RedirectToAction("Index", new {id = dbSetName });
            }
            
            ViewBag.DbSetName = id;

            ViewBag.IgnoreFromForm = databaseGeneratedProperties;

            ViewBag.Relationships = relationships;

            return View("Create", newEntity);
        }

        [HttpGet]
        [IgnoreAntiforgeryToken]
        public IActionResult Create(string id)
        {
            var dbSetValue = GetDbSetValueOrNull(id, out var dbContextObject, out var entityType, out var relationships);

            var newEntity = System.Activator.CreateInstance(entityType);
            ViewBag.DbSetName = id;

            var autoGeneratedPropertyNames =
                newEntity.GetType().GetProperties()
                .Where(p => p.GetCustomAttributes().Any(a => a.GetType().Name.Contains("DatabaseGenerated"))).Select(p => p.Name);

            ViewBag.IgnoreFromForm = autoGeneratedPropertyNames;
            ViewBag.Relationships = relationships;
            return View(newEntity);
        }

        [HttpGet]
        public IActionResult EditEntity(string dbSetName, string id)
        {
            var entityToEdit = GetEntityFromDbSet(dbSetName, id, out var dbContextObject, out var entityType, out var relationships);

            var databaseGeneratedProperties =
            entityToEdit.GetType().GetProperties()
            .Where(p => p.GetCustomAttributes().Any(a => a.GetType().Name.Contains("DatabaseGenerated"))).Select(p => p.Name);

            ViewBag.DbSetName = dbSetName;
            ViewBag.Id = id;
            ViewBag.Relationships = relationships;
            ViewBag.IgnoreFromForm = databaseGeneratedProperties;
            return View("Edit", entityToEdit);
        }



        [HttpPost]
        public async Task<IActionResult> EditEntityPost(string dbSetName, string id, [FromForm] object formData)
        {
            var entityToEdit = GetEntityFromDbSet(dbSetName, id, out var dbContextObject, out var entityType, out var relationships);

            dbContextObject.Attach(entityToEdit);

            await AddByteArrayFiles(entityToEdit);

            var databaseGeneratedProperties =
           entityToEdit.GetType().GetProperties()
           .Where(p => p.GetCustomAttributes().Any(a => a.GetType().Name.Contains("DatabaseGenerated"))).Select(p => p.Name);

            await TryUpdateModelAsync(entityToEdit, entityType, string.Empty, await CompositeValueProvider.CreateAsync(this.ControllerContext, this.ControllerContext.ValueProviderFactories),
                (ModelMetadata meta) => !databaseGeneratedProperties.Contains(meta.PropertyName));

            // remove any errors from fk properties - ef will handle this validation
            foreach (var fkProperty in entityToEdit.GetType().GetProperties()
            .Where(p => p.GetCustomAttributes().Any(a => a.GetType().Name.Contains("ForeignKey"))).Select(p => p.Name))
            {
                if (ModelState.ContainsKey(fkProperty))
                {
                    ModelState[fkProperty].Errors.Clear();
                    ModelState[fkProperty].ValidationState = ModelValidationState.Skipped;
                }
            }

            if (ModelState.ValidationState == ModelValidationState.Valid)
            {
                await dbContextObject.SaveChangesAsync();
                return RedirectToAction("Index", new { id = dbSetName });
            }

            ViewBag.DbSetName = dbSetName;
            ViewBag.Id = id;
            ViewBag.Relationships = relationships;
            ViewBag.IgnoreFromForm = databaseGeneratedProperties;

            return View("Edit", entityToEdit);
        }

        private async Task AddByteArrayFiles(object entityToEdit)
        {
            foreach (var file in Request.Form.Files)
            {
                var matchingProperty = entityToEdit.GetType().GetProperties()
                    .FirstOrDefault(prop => prop.Name == file.Name && prop.PropertyType == typeof(byte[]));
                if (matchingProperty != null)
                {
                    var memoryStream = new MemoryStream();
                    await file.CopyToAsync(memoryStream);
                    matchingProperty.SetValue(entityToEdit, memoryStream.ToArray());
                }
            }
        }

        [HttpGet]
        public IActionResult DeleteEntity(string dbSetName, string id)
        {
            var viewModel = new DataDeleteViewModel();
            viewModel.DbSetName = dbSetName;
            viewModel.Id = id;
            viewModel.Object = GetEntityFromDbSet(dbSetName, id, out var dbContext, out var entityType, out var relationships);
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
