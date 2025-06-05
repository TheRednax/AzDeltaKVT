using AzDeltaKVT.Core;
using AzDektaKVT.Model;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AzDeltaKVT.Services
{
    public class TranscriptService
    {
        private readonly AzDeltaKVTDbContext _context;

        public TranscriptService(AzDeltaKVTDbContext context)
        {
            _context = context;
        }

        public async Task<NmTranscript?> GetByIdAsync(string nmNumber)
        {
            return await _context.NmTranscripts
                .Include(t => t.Gene)
                .FirstOrDefaultAsync(t => t.NmNumber == nmNumber);
        }

        public async Task<bool> UpdateAsync(string nmNumber, NmTranscript transcript)
        {
            var existing = await _context.NmTranscripts.FindAsync(nmNumber);
            if (existing == null) return false;

            // Update fields except key
            existing.GeneId = transcript.GeneId;
            existing.IsSelect = transcript.IsSelect;
            existing.IsClinical = transcript.IsClinical;
            existing.IsInHouse = transcript.IsInHouse;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(string nmNumber)
        {
            var transcript = await _context.NmTranscripts.FindAsync(nmNumber);
            if (transcript == null) return false;

            _context.NmTranscripts.Remove(transcript);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
