//using AutoMapper;
//using CSharpFunctionalExtensions;
//using FamilyBudget.Core.Interfaces;
//using FamilyBudget.Core.Models;
//using FamilyBudget.Infrastructure.Entities;
//using Microsoft.EntityFrameworkCore;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Telegram.Bot.Types;

//namespace FamilyBudget.Infrastructure.Repositories
//{
//    public class ReceiptRepository 
//    {
//        private readonly ApplicationDbContext _context;
//        private readonly IMapper _mapper;

//        public ReceiptRepository(ApplicationDbContext context, IMapper mapper)
//        {
//            _context = context;
//            _mapper = mapper;
//        }

//        public async Task<IEnumerable<Receipt>> GetByUserIdAsync(Guid userId)
//        {
//            var receiptEntities = await _context.Receipts
//                .AsNoTracking()
//                .Where(r => r.UserId == userId)
//                .ToListAsync();

//            return _mapper.Map<IEnumerable<Receipt>>(receiptEntities);
//        }

//        public async Task<IEnumerable<Receipt>> GetByFamilyIdAsync(Guid familyId)
//        {
//            var receiptEntities = await _context.Receipts
//                .AsNoTracking()
//                .Where(r => r.FamilyId == familyId)
//                .ToListAsync();

//            return _mapper.Map<IEnumerable<Receipt>>(receiptEntities);
//        }

//        public async Task<IEnumerable<Receipt>> GetUnprocessedReceiptsAsync()
//        {
//            var receiptEntities = await _context.Receipts
//                .AsNoTracking()
//                .Where(r => !r.IsProcessed)
//                .ToListAsync();

//            return _mapper.Map<IEnumerable<Receipt>>(receiptEntities);
//        }

//        public async Task<Guid> AddAsync(Receipt receipt)
//        {
//            var receiptEntity = _mapper.Map<ReceiptEntity>(receipt);

//            _context.Receipts.Add(receiptEntity);
//            await _context.SaveChangesAsync();

//            return receipt.Id;
//        }
//        public async Task<Guid> MarkAsProcessedAsync(Guid id, decimal totalAmount)
//        {
//            await _context.Receipts
//                .Where(r => r.Id == id)
//                .ExecuteUpdateAsync(setters => setters
//                    .SetProperty(r => r.IsProcessed, true)
//                    .SetProperty(r => r.TotalAmount, totalAmount));

//            return id;
//        }

//    }
//}
