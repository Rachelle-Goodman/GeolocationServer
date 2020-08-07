using Geolocation.Entities;
using Geolocation.Factory;
using Geoloocation.DB;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Geolocation.BL
{
    internal static class PlaceDbCache
    {
        private static readonly Dictionary<string, PlaceDbDto> _nameToPlace;
        private static readonly IDB _db;

        static PlaceDbCache()
        {
            _nameToPlace = new Dictionary<string, PlaceDbDto>();
            _db = DbFactory.DB;
        }

        internal static async Task InsertPlace(PlaceDbDto place)
        {
            _nameToPlace[place.PlaceName] = place;
            await _db.Insert(place);
        }

        internal static async Task<PlaceDbDto> GetPlace(string place)
        {
            if (!_nameToPlace.ContainsKey(place))
            {
                var placeDbDto = await GetPlaceFromDB(place);

                if (placeDbDto != null)
                {
                    _nameToPlace[place] = placeDbDto;
                }

                return placeDbDto;
            }

            return _nameToPlace[place];
        }

        private static async Task<PlaceDbDto> GetPlaceFromDB(string place)
            => await _db.Get<PlaceDbDto>(place);
    }
}
