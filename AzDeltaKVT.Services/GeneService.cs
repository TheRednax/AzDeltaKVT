using AzDektaKVT.Model;
using AzDeltaKVT.Core;
using AzDeltaKVT.Dto.Requests;
using AzDeltaKVT.Dto.Results;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AzDeltaKVT.Services
{
    public class GeneService
    {
        private readonly AzDeltaKVTDbContext _context;

        public GeneService(AzDeltaKVTDbContext context)
        {
            _context = context;
        }

        public async Task<IList<GeneResult>> Find()
        {
            var query = _context.Genes
                .Select(o => new GeneResult
                {
                    Name = o.Name,
                    Chromosome = o.Chromosome,
                    Start = o.Start,
                    Stop = o.Stop,
                });

            var genes = await query
                .ToListAsync();

            return genes;
        }

        public async Task<Gene> Get(GeneRequest request)
        {
            var query = _context.Genes.AsQueryable();

            if (!string.IsNullOrEmpty(request.Name))
            {
                query = query.Where(g => g.Name.Contains(request.Name));
            }
            else if (!string.IsNullOrEmpty(request.Nm_Number))
            {
                // Lookup the geneId using the NM number
                var geneId = await _context.NmTranscripts
                    .Where(t => t.NmNumber == request.Nm_Number)
                    .Select(t => t.GeneId)
                    .FirstOrDefaultAsync();

                if (!string.IsNullOrEmpty(geneId))
                {
                    query = query.Where(g => g.Name == geneId);
                }
                else
                {
                    return new GeneResult(); // or return null / NotFound
                }
            }
            else if (!string.IsNullOrEmpty(request.Chromosome) && request.Position.HasValue)
            {
                query = query.Where(g =>
                    g.Chromosome == request.Chromosome &&
                    g.Start <= request.Position &&
                    g.Stop >= request.Position);
            }
            else
            {
                return new GeneResult(); // No valid filter applied
            }

            var result = await query
                .Select(g => new GeneResult
                {
                    Name = g.Name,
                    Chromosome = g.Chromosome,
                    Start = g.Start,
                    Stop = g.Stop,
                    UserInfo = g.UserInfo
                })
                .FirstOrDefaultAsync();

            return result ?? new GeneResult(); // fallback
        }

        public async Task<GeneResult> Create(GeneRequest request)
        {
            var result = new Gene
            {
                Name = request.Name,
                Chromosome = request.Chromosome,
                Start = request.Start,
                Stop = request.Stop,
                UserInfo = request.UserInfo
            };

            _context.Genes.Add(result);
            await _context.SaveChangesAsync();
            return new GeneResult
            {
                Name = result.Name,
                Chromosome = result.Chromosome,
                Start = result.Start,
                Stop = result.Stop,
                UserInfo = result.UserInfo
            }; ;
        }

        public async Task<GeneResult> Update(GeneRequest gene)
        {
            var existing = await _context.Genes.FindAsync(gene.Name);
            if (existing == null) return null;

            existing.Chromosome = gene.Chromosome;
            existing.Start = gene.Start;
            existing.Stop = gene.Stop;
            existing.UserInfo = gene.UserInfo;

            await _context.SaveChangesAsync();

            return new GeneResult
            {
                Name = existing.Name,
                Chromosome = existing.Chromosome,
                Start = existing.Start,
                Stop = existing.Stop,
                UserInfo = existing.UserInfo
            };
        }

        public async Task<bool> Delete(string name)
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