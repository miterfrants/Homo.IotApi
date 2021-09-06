using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Homo.IotApi
{
    public class ZoneDataservice
    {
        public static List<Zone> GetList(IotDbContext dbContext, int page, int limit)
        {
            return dbContext.Zone
                .Where(x =>
                    x.DeletedAt == null
                )
                .OrderByDescending(x => x.Id)
                .Skip(limit * (page - 1))
                .Take(limit)
                .ToList();
        }

        public static List<Zone> GetAll(IotDbContext dbContext)
        {
            return dbContext.Zone
                .Where(x =>
                    x.DeletedAt == null
                )
                .OrderByDescending(x => x.Id)
                .ToList();
        }
        public static int GetRowNum(IotDbContext dbContext)
        {
            return dbContext.Zone
                .Where(x =>
                    x.DeletedAt == null
                )
                .Count();
        }

        public static Zone GetOne(IotDbContext dbContext, long id)
        {
            return dbContext.Zone.FirstOrDefault(x => x.DeletedAt == null && x.Id == id);
        }

        public static Zone Create(IotDbContext dbContext, long createdBy, DTOs.Zone dto)
        {
            Zone record = new Zone();
            foreach (var propOfDTO in dto.GetType().GetProperties())
            {
                var value = propOfDTO.GetValue(dto);
                var prop = record.GetType().GetProperty(propOfDTO.Name);
                prop.SetValue(record, value);
            }
            record.CreatedBy = createdBy;
            dbContext.Zone.Add(record);
            dbContext.SaveChanges();
            return record;
        }

        public static void BatchDelete(IotDbContext dbContext, long editedBy, List<long> ids)
        {
            foreach (long id in ids)
            {
                Zone record = new Zone { Id = id };
                dbContext.Attach<Zone>(record);
                record.DeletedAt = DateTime.Now;
                record.EditedBy = editedBy;
            }
            dbContext.SaveChanges();
        }

        public static void Update(IotDbContext dbContext, long id, long editedBy, DTOs.Zone dto)
        {
            Zone record = dbContext.Zone.Where(x => x.Id == id).FirstOrDefault();
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
            Zone record = dbContext.Zone.Where(x => x.Id == id).FirstOrDefault();
            record.DeletedAt = DateTime.Now;
            record.EditedBy = editedBy;
            dbContext.SaveChanges();
        }
    }
}
