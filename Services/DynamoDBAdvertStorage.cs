using AdvertApi.Models;
using AutoMapper;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;

namespace AdvertApi.Services
{
    public class DynamoDBAdvertStorage : IAdvertStorageService
    {
        private readonly IMapper _mapper;
        public DynamoDBAdvertStorage(IMapper mapper)
        {
           _mapper = mapper;   
        }
        public async Task<string> Add(AdvertModel model)
        {
            var dbmodel= _mapper.Map<AdvertDbModel>(model);
            dbmodel.Id = new Guid().ToString();
            dbmodel.CreationDateTime = DateTime.UtcNow;
            dbmodel.Status = AdvertStatus.Pending;
            using (var client = new AmazonDynamoDBClient())
            {
                using (var context= new DynamoDBContext(client)) 
                {
                 await  context.SaveAsync(dbmodel); 

                }
            }
            return dbmodel.Id;   
        }

        public async Task Confirm(ConfirmAdvertModel model)
        {
            using (var client = new AmazonDynamoDBClient())
            {
                using (var context = new DynamoDBContext(client))
                {

                    var record =await context.LoadAsync<AdvertDbModel>(model.Id);
                    if (record==null)
                    {
                        throw new KeyNotFoundException($"A record with ID= {model.Id} was not found ");
                    }

                    if (model.Status==AdvertStatus.Active)
                    {
                        record.Status = AdvertStatus.Active;
                        await context.SaveAsync(record);
                    }

                    else
                    {
                        await context.DeleteAsync(record);  
                    }
                }
            }

        }
    }
}
