using Geolocation.DependencyInjection;
using Geolocation.Entities;
using Geoloocation.DB;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Geolocation.BL
{
    internal interface IPlaceDbCache
    {
        Task InsertPlace(PlaceDbDto place);
        Task<PlaceDbDto> GetPlace(string place);
    }

    [DependencyInjection(DependencyInjectionType.Singleton)]
    internal class PlaceDbCache: IPlaceDbCache
    {
        private static readonly Dictionary<string, PlaceDbDto> _nameToPlace;
        private static IDB _db;

        static PlaceDbCache()
        {
            _nameToPlace = new Dictionary<string, PlaceDbDto>();
        }

        public PlaceDbCache(IDB db)
        {
            _db = db;
        }

        public async Task InsertPlace(PlaceDbDto place)
        {
            _nameToPlace[place.PlaceName] = place;
            await _db.Insert(place);
        }

        public async Task<PlaceDbDto> GetPlace(string place)
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
