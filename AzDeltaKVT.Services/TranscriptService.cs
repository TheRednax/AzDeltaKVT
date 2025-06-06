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

        public async Task<IList<NmTranscript>> Find()
        {
            return await _context.NmTranscripts
                .Include(t => t.Gene)
                .ToListAsync();
        }

        public async Task<NmTranscript?> Get(string nmNumber)
        {
            return await _context.NmTranscripts
                .Include(t => t.Gene)
                .FirstOrDefaultAsync(t => t.NmNumber == nmNumber);
        }

        public async Task<NmTranscript> Create(NmTranscript transcript)
        {
            _context.NmTranscripts.Add(transcript);
            await _context.SaveChangesAsync();
            return transcript;
        }

        public async Task<bool> Update(NmTranscript transcript)
        {
            var existing = await _context.NmTranscripts.FindAsync(transcript.NmNumber);
            if (existing == null) return false;

            existing.GeneId = transcript.GeneId;
            existing.IsSelect = transcript.IsSelect;
            existing.IsClinical = transcript.IsClinical;
            existing.IsInHouse = transcript.IsInHouse;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> Delete(string nmNumber)
        {
            var transcript = await _context.NmTranscripts.FindAsync(nmNumber);
            if (transcript == null) return false;

            _context.NmTranscripts.Remove(transcript);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
