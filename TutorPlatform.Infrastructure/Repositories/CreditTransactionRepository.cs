using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TutorPlatform.Domain.Common;
using TutorPlatform.Domain.Entities;
using TutorPlatform.Domain.Enums;
using TutorPlatform.Domain.Interfaces;
using TutorPlatform.Infrastructure.Models;
using TutorPlatform.Infrastructure.Persistence;

namespace TutorPlatform.Infrastructure.Repositories
{
    public class CreditTransactionRepository : ICreditTransactionRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public CreditTransactionRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<CreditTransaction?> GetByIdAsync(Guid id)
        {
            var dataModel = await _dbContext.CreditTransactions
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (dataModel == null) return null;

            return MapToDomain(dataModel);
        }

        public async Task<PagedResult<CreditTransaction>> GetByUserIdAsync(Guid userId, int pageNumber, int pageSize)
        {
            var query = _dbContext.CreditTransactions
                .AsNoTracking()
                .Where(x => x.UserId == userId);

            var totalCount = await query.CountAsync();
            var itemsData = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var domainItems = itemsData.Select(MapToDomain).ToList();

            return new PagedResult<CreditTransaction>(domainItems, totalCount);
        }

        public async Task<CreditTransaction> AddAsync(CreditTransaction transaction)
        {
            var dataModel = new CreditTransactionDataModel
            {
                Id = transaction.Id,
                UserId = transaction.UserId,
                Amount = transaction.Amount,
                Type = MapToDbType(transaction.Type),
                Description = transaction.Description,
                BookingId = transaction.BookingId,
                BalanceAfter = transaction.BalanceAfter,
                CreatedAt = transaction.CreatedAt,
                UpdatedAt = transaction.UpdatedAt
            };

            await _dbContext.CreditTransactions.AddAsync(dataModel);
            await _dbContext.SaveChangesAsync();

            return transaction;
        }

        private CreditTransaction MapToDomain(CreditTransactionDataModel dataModel)
        {
            var domainType = MapToDomainType(dataModel.Type);
            var domainTx = new CreditTransaction(
                dataModel.UserId,
                dataModel.Amount,
                domainType,
                dataModel.Description,
                dataModel.BalanceAfter,
                dataModel.BookingId);

            // Set Id, CreatedAt and UpdatedAt using reflection
            typeof(CreditTransaction).GetProperty("Id")?.SetValue(domainTx, dataModel.Id);
            typeof(CreditTransaction).GetProperty("CreatedAt")?.SetValue(domainTx, dataModel.CreatedAt);
            typeof(CreditTransaction).GetProperty("UpdatedAt")?.SetValue(domainTx, dataModel.UpdatedAt);

            return domainTx;
        }

        private CreditTransactionType MapToDomainType(int dbType)
        {
            return dbType switch
            {
                1 => CreditTransactionType.Credit,
                2 => CreditTransactionType.Debit,
                3 => CreditTransactionType.Refund,
                4 => CreditTransactionType.Transfer,
                _ => CreditTransactionType.Credit
            };
        }

        private int MapToDbType(CreditTransactionType domainType)
        {
            return domainType switch
            {
                CreditTransactionType.Credit => 1,
                CreditTransactionType.Debit => 2,
                CreditTransactionType.Refund => 3,
                CreditTransactionType.Transfer => 4,
                _ => 1
            };
        }
    }
}
