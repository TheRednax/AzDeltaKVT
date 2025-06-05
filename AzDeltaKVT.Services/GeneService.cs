using AzDektaKVT.Model;
using AzDeltaKVT.Core;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzDeltaKVT.Services
{
    public class GeneService
    {
        private readonly AzDeltaKVTDbContext _context;

        public GeneService(AzDeltaKVTDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Gene>> GetAllAsync(string? name)
        {
            var query = _context.Genes.AsQueryable();

            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(g => g.Name.Contains(name));
            }

            return await query.ToListAsync();
        }

        public async Task<Gene?> GetByIdAsync(string name)
        {
            return await _context.Genes
                .FirstOrDefaultAsync(g => g.Name == name);
        }

        public async Task<Gene> CreateAsync(Gene gene)
        {
            _context.Genes.Add(gene);
            await _context.SaveChangesAsync();
            return gene;
        }

        public async Task<bool> UpdateAsync(string name, Gene gene)
        {
            var existing = await _context.Genes.FindAsync(name);
            if (existing == null) return false;

            existing.Chromosome = gene.Chromosome;
            existing.Start = gene.Start;
            existing.Stop = gene.Stop;
            existing.UserInfo = gene.UserInfo;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(string name)
        {
            var gene = await _context.Genes.FindAsync(name);
            if (gene == null)
                return false;

            _context.Genes.Remove(gene);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}