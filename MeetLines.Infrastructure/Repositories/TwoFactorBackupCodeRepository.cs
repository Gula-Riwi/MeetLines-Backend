using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MeetLines.Domain.Entities;
using MeetLines.Domain.Repositories;
using MeetLines.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MeetLines.Infrastructure.Repositories
{
    public class TwoFactorBackupCodeRepository : ITwoFactorBackupCodeRepository
    {
        private readonly MeetLinesPgDbContext _context;

        public TwoFactorBackupCodeRepository(MeetLinesPgDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<List<TwoFactorBackupCode>> GenerateBackupCodesAsync(
            Guid userId, 
            string userType, 
            int count = 10, 
            CancellationToken ct = default)
        {
            // Primero elimina los códigos anteriores
            await DeleteAllByUserAsync(userId, userType, ct);

            // Genera nuevos códigos
            var codes = new List<TwoFactorBackupCode>();
            var codeStrings = new HashSet<string>();

            // Genera códigos únicos
            var random = new Random();
            while (codeStrings.Count < count)
            {
                var code = GenerateRandomCode();
                codeStrings.Add(code);
            }

            // Crea las entidades
            foreach (var codeString in codeStrings)
            {
                var backupCode = new TwoFactorBackupCode(userId, userType, codeString);
                codes.Add(backupCode);
                await _context.TwoFactorBackupCodes.AddAsync(backupCode, ct);
            }

            await _context.SaveChangesAsync(ct);
            return codes;
        }

        public async Task<bool> ValidateAndUseBackupCodeAsync(
            Guid userId, 
            string userType, 
            string code, 
            CancellationToken ct = default)
        {
            var backupCode = await _context.TwoFactorBackupCodes
                .FirstOrDefaultAsync(bc => 
                    bc.UserId == userId && 
                    bc.UserType == userType && 
                    bc.Code == code && 
                    !bc.IsUsed, 
                    ct);

            if (backupCode == null)
                return false;

            backupCode.MarkAsUsed();
            await _context.SaveChangesAsync(ct);
            return true;
        }

        public async Task<List<TwoFactorBackupCode>> GetByUserAsync(
            Guid userId, 
            string userType, 
            CancellationToken ct = default)
        {
            return await _context.TwoFactorBackupCodes
                .Where(bc => bc.UserId == userId && bc.UserType == userType)
                .OrderBy(bc => bc.CreatedAt)
                .ToListAsync(ct);
        }

        public async Task DeleteAllByUserAsync(
            Guid userId, 
            string userType, 
            CancellationToken ct = default)
        {
            var codes = await _context.TwoFactorBackupCodes
                .Where(bc => bc.UserId == userId && bc.UserType == userType)
                .ToListAsync(ct);

            _context.TwoFactorBackupCodes.RemoveRange(codes);
            await _context.SaveChangesAsync(ct);
        }

        public async Task<int> CountUnusedCodesAsync(
            Guid userId, 
            string userType, 
            CancellationToken ct = default)
        {
            return await _context.TwoFactorBackupCodes
                .CountAsync(bc => 
                    bc.UserId == userId && 
                    bc.UserType == userType && 
                    !bc.IsUsed, 
                    ct);
        }

        private string GenerateRandomCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            var code = new char[8];
            
            for (int i = 0; i < 8; i++)
            {
                code[i] = chars[random.Next(chars.Length)];
            }
            
            return $"{new string(code, 0, 4)}-{new string(code, 4, 4)}";
        }
    }
}
