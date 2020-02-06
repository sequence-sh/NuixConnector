using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Razor_Components;
using TG.Blazor.IndexedDB;

namespace NuixFrontEnd.Shared
{
    public class Datastore : IDatastore
    {
        public const string DbName = "NuixMethodsDatastore";

        private readonly IndexedDBManager _dbManager;

        public Datastore(IndexedDBManager dbManager)
        {
            _dbManager = dbManager;
        }

        public async Task<bool> AddOrUpdate(MethodMetadata methodMetadata, IEnumerable<ParameterMetadata> parameters)
        {
            //TODO the updates aren't working - they're just adding

            var (existingMethod, existingParameters) =
                await GetMetadata(methodMetadata.ClassAndMethod);

            var tasks = new List<Task>();

            var methodRecord = new StoreRecord<MethodMetadata>
                {Data = methodMetadata, Storename = nameof(MethodMetadata)};

            if (existingMethod != null)
            {
                methodMetadata.Id = existingMethod.Id;

                tasks.Add(_dbManager.UpdateRecord(methodRecord));
            }
            else
            {
                tasks.Add(_dbManager.AddRecord(methodRecord));
            }
            
            foreach (var parameterMetadata in parameters)
            {
                if (parameterMetadata.PClassAndMethod != methodMetadata.ClassAndMethod)
                    throw new ArgumentException("Parameters must belong to the method");

                var parameterRecord = new StoreRecord<ParameterMetadata>()
                    {Data = parameterMetadata, Storename = nameof(ParameterMetadata)};
                if (existingParameters != null && existingParameters.TryGetValue(parameterMetadata.ParameterName,
                    out var ep))
                {
                    parameterMetadata.Id = ep.Id;

                    tasks.Add(_dbManager.UpdateRecord(parameterRecord));
                }
                else
                {
                    tasks.Add(_dbManager.AddRecord(parameterRecord));
                }
            }

            await Task.WhenAll(tasks);
            return true;
        }
            
        public async Task<(MethodMetadata? method, IReadOnlyDictionary<string, ParameterMetadata>? parameters)> GetMetadata(string classAndMethod)
        {

            var allMethods = await _dbManager.GetRecords<MethodMetadata>(nameof(MethodMetadata));
            var allParameters = await _dbManager.GetRecords<ParameterMetadata >(nameof(ParameterMetadata));

            //TODO make the indexes work!

            //var methodQuery = new StoreIndexQuery<string>
            //{
            //    Storename = nameof(MethodMetadata),
            //    IndexName = nameof(MethodMetadata.ClassAndMethod),
            //    QueryValue = classAndMethod
            //};

            //var methodTask = _dbManager.GetRecordByIndex<string, MethodMetadata>(methodQuery);
            
            //var parameterQuery = new StoreIndexQuery<string>
            //{
            //    Storename = nameof(ParameterMetadata),
            //    IndexName = nameof(ParameterMetadata.PClassAndMethod),
            //    QueryValue = classAndMethod
            //};

            //var parametersTask = _dbManager.GetAllRecordsByIndex<string, ParameterMetadata>(parameterQuery);
            
            //await Task.WhenAll(methodTask, parametersTask); //run both queries in parallel

            var methodMetadata = allMethods.OrderByDescending(x=>x.Id).FirstOrDefault(x => x.ClassAndMethod == classAndMethod);

            var parameterMetadata = 
                allParameters.Where(x=>x.PClassAndMethod == classAndMethod )
                    .GroupBy(x=>x.ParameterName).Select(x=>x.OrderByDescending(p=>p.Id).First())
                    .ToDictionary(x => x.ParameterName);

            return (methodMetadata, parameterMetadata);
        }
    }
}
