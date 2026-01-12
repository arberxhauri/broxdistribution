// Repositories/ContactRepository.cs
using BroxDistribution.Models;
using BroxDistribution1.Repositories;

namespace BroxDistribution.Repositories
{
    public class ContactRepository : JsonFileRepository<ContactForm>
    {
        public ContactRepository(IWebHostEnvironment environment) 
            : base(environment, "contacts.json")
        {
        }

        public async Task<IEnumerable<ContactForm>> GetUnreadAsync()
        {
            var contacts = await GetAllAsync();
            return contacts.Where(c => !c.IsRead).OrderByDescending(c => c.SubmittedAt);
        }

        public async Task<IEnumerable<ContactForm>> GetAllOrderedAsync()
        {
            var contacts = await GetAllAsync();
            return contacts.OrderByDescending(c => c.SubmittedAt);
        }

        public async Task MarkAsReadAsync(int id)
        {
            var contact = await GetByIdAsync(id);
            if (contact != null)
            {
                contact.IsRead = true;
                await UpdateAsync(contact);
            }
        }
    }
}