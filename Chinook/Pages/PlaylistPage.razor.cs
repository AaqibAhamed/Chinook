using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Chinook.Pages
{
    public partial class PlaylistPage
    {
        [Parameter] public long PlaylistId { get; set; }
        [Inject] IDbContextFactory<ChinookContext> DbFactory { get; set; }
        [CascadingParameter] private Task<AuthenticationState> authenticationState { get; set; }

        private Chinook.ClientModels.Playlist Playlist;
        private string CurrentUserId;
        private string InfoMessage;

        protected override async Task OnInitializedAsync()
        {
            CurrentUserId = await GetUserId();

            await InvokeAsync(StateHasChanged);
            var DbContext = await DbFactory.CreateDbContextAsync();

            Playlist = DbContext.Playlists
                .Include(a => a.Tracks).ThenInclude(a => a.Album).ThenInclude(a => a.Artist)
                .Where(p => p.PlaylistId == PlaylistId)
                .Select(p => new ClientModels.Playlist()
                {
                    Name = p.Name,
                    Tracks = p.Tracks.Select(t => new ClientModels.PlaylistTrack()
                    {
                        AlbumTitle = t.Album.Title,
                        ArtistName = t.Album.Artist.Name,
                        TrackId = t.TrackId,
                        TrackName = t.Name,
                        IsFavorite = t.Playlists.Where(p => p.UserPlaylists.Any(up => up.UserId == CurrentUserId && up.Playlist.Name == "Favorites")).Any()
                    }).ToList()
                })
                .FirstOrDefault();
        }

        private async Task<string> GetUserId()
        {
            var user = (await authenticationState).User;
            var userId = user.FindFirst(u => u.Type.Contains(ClaimTypes.NameIdentifier))?.Value;
            return userId;
        }

        private void FavoriteTrack(long trackId)
        {
            var track = Playlist.Tracks.FirstOrDefault(t => t.TrackId == trackId);
            InfoMessage = $"Track {track.ArtistName} - {track.AlbumTitle} - {track.TrackName} added to playlist Favorites.";
        }

        private void UnfavoriteTrack(long trackId)
        {
            var track = Playlist.Tracks.FirstOrDefault(t => t.TrackId == trackId);
            InfoMessage = $"Track {track.ArtistName} - {track.AlbumTitle} - {track.TrackName} removed from playlist Favorites.";
        }

        private void RemoveTrack(long trackId)
        {
            CloseInfoMessage();

        }

        private void CloseInfoMessage()
        {
            InfoMessage = "";
        }

    }
}
