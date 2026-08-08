using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TutorPlatform.Application.Common.Exceptions;
using TutorPlatform.Application.Common.Interfaces;
using TutorPlatform.Infrastructure.Models;
using TutorPlatform.Infrastructure.Persistence;

namespace TutorPlatform.Infrastructure.Services
{
    public class CreditService : ICreditService
    {
        private readonly ApplicationDbContext _dbContext;

        public CreditService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task DebitAsync(Guid userId, decimal amount, string description, Guid? bookingId)
        {
            var ownsTransaction = _dbContext.Database.CurrentTransaction == null;
            var transaction = ownsTransaction
                ? await _dbContext.Database.BeginTransactionAsync()
                : _dbContext.Database.CurrentTransaction!;
            try
            {
                var user = await _dbContext.Users.FindAsync(userId);
                if (user == null) throw new NotFoundException(nameof(Domain.Entities.User), userId);

                if (user.CreditBalance < amount) throw new BadRequestException("Insufficient credits.");

                user.CreditBalance -= amount;

                var tx = new CreditTransactionDataModel
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Amount = -amount,
                    Type = 2, // 2 = Debit
                    Description = description,
                    BookingId = bookingId,
                    CreatedAt = DateTime.UtcNow
                };

                await _dbContext.CreditTransactions.AddAsync(tx);
                _dbContext.Users.Update(user);
                await _dbContext.SaveChangesAsync();

                if (ownsTransaction) await transaction.CommitAsync();
            }
            catch
            {
                if (ownsTransaction) await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task TransferAsync(Guid fromUserId, Guid toUserId, decimal amount, string description, Guid bookingId)
        {
            var ownsTransaction = _dbContext.Database.CurrentTransaction == null;
            var transaction = ownsTransaction
                ? await _dbContext.Database.BeginTransactionAsync()
                : _dbContext.Database.CurrentTransaction!;
            try
            {
                var toUser = await _dbContext.Users.FindAsync(toUserId);
                if (toUser == null) throw new NotFoundException(nameof(Domain.Entities.User), toUserId);

                toUser.CreditBalance += amount;

                var tx = new CreditTransactionDataModel
                {
                    Id = Guid.NewGuid(),
                    UserId = toUserId,
                    Amount = amount,
                    Type = 4, // 4 = Transfer (received)
                    Description = description,
                    BookingId = bookingId,
                    CreatedAt = DateTime.UtcNow
                };

                await _dbContext.CreditTransactions.AddAsync(tx);
                _dbContext.Users.Update(toUser);
                await _dbContext.SaveChangesAsync();

                if (ownsTransaction) await transaction.CommitAsync();
            }
            catch
            {
                if (ownsTransaction) await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task RefundAsync(Guid userId, decimal amount, string description, Guid bookingId)
        {
            var ownsTransaction = _dbContext.Database.CurrentTransaction == null;
            var transaction = ownsTransaction
                ? await _dbContext.Database.BeginTransactionAsync()
                : _dbContext.Database.CurrentTransaction!;
            try
            {
                var user = await _dbContext.Users.FindAsync(userId);
                if (user == null) throw new NotFoundException(nameof(Domain.Entities.User), userId);

                user.CreditBalance += amount;

                var tx = new CreditTransactionDataModel
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Amount = amount,
                    Type = 3, // 3 = Refund
                    Description = description,
                    BookingId = bookingId,
                    CreatedAt = DateTime.UtcNow
                };

                await _dbContext.CreditTransactions.AddAsync(tx);
                _dbContext.Users.Update(user);
                await _dbContext.SaveChangesAsync();

                if (ownsTransaction) await transaction.CommitAsync();
            }
            catch
            {
                if (ownsTransaction) await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
