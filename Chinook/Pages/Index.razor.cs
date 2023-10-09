using Chinook.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace Chinook.Pages
{
    public partial class Index
    {
        private List<Artist> Artists;
        [Inject] IDbContextFactory<ChinookContext> DbFactory { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await InvokeAsync(StateHasChanged);
            Artists = await GetArtists();
        }

        public async Task<List<Artist>> GetArtists()
        {
            var dbContext = await DbFactory.CreateDbContextAsync();
            var users = dbContext.Users.Include(a => a.UserPlaylists).ToList();

            return dbContext.Artists.ToList();
        }

        public async Task<List<Album>> GetAlbumsForArtist(int artistId)
        {
            var dbContext = await DbFactory.CreateDbContextAsync();
            return dbContext.Albums.Where(a => a.ArtistId == artistId).ToList();
        }
    }
}
