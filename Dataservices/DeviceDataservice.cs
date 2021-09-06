using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Homo.IotApi
{
    public class DeviceDataservice
    {
        public static List<Device> GetList(IotDbContext dbContext, int page, int limit)
        {
            return dbContext.Device
                .Where(x =>
                    x.DeletedAt == null
                )
                .OrderByDescending(x => x.Id)
                .Skip(limit * (page - 1))
                .Take(limit)
                .ToList();
        }

        public static List<Device> GetAll(IotDbContext dbContext)
        {
            return dbContext.Device
                .Where(x =>
                    x.DeletedAt == null
                )
                .OrderByDescending(x => x.Id)
                .ToList();
        }
        public static int GetRowNum(IotDbContext dbContext)
        {
            return dbContext.Device
                .Where(x =>
                    x.DeletedAt == null
                )
                .Count();
        }

        public static Device GetOne(IotDbContext dbContext, long id)
        {
            return dbContext.Device.FirstOrDefault(x => x.DeletedAt == null && x.Id == id);
        }

        public static Device Create(IotDbContext dbContext, long createdBy, DTOs.Device dto)
        {
            Device record = new Device();
            foreach (var propOfDTO in dto.GetType().GetProperties())
            {
                var value = propOfDTO.GetValue(dto);
                var prop = record.GetType().GetProperty(propOfDTO.Name);
                prop.SetValue(record, value);
            }
            record.CreatedBy = createdBy;
            dbContext.Device.Add(record);
            dbContext.SaveChanges();
            return record;
        }

        public static void BatchDelete(IotDbContext dbContext, long editedBy, List<long> ids)
        {
            foreach (long id in ids)
            {
                Device record = new Device { Id = id };
                dbContext.Attach<Device>(record);
                record.DeletedAt = DateTime.Now;
                record.EditedBy = editedBy;
            }
            dbContext.SaveChanges();
        }

        public static void Update(IotDbContext dbContext, long id, long editedBy, DTOs.Device dto)
        {
            Device record = dbContext.Device.Where(x => x.Id == id).FirstOrDefault();
            foreach (var propOfDTO in dto.GetType().GetProperties())
            {
                var value = propOfDTO.GetValue(dto);
                var prop = record.GetType().GetProperty(propOfDTO.Name);
                prop.SetValue(record, value);
            }
            record.EditedAt = DateTime.Now;
            record.EditedBy = editedBy;
            dbContext.SaveChanges();
        }

        public static void Delete(IotDbContext dbContext, long id, long editedBy)
        {
            Device record = dbContext.Device.Where(x => x.Id == id).FirstOrDefault();
            record.DeletedAt = DateTime.Now;
            record.EditedBy = editedBy;
            dbContext.SaveChanges();
        }
    }
}
