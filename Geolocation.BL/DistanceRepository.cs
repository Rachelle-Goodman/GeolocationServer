using Geolocation.Entities;
using Geolocation.Factory;
using Geolocation.Utilities.Google;
using Geolocation.Utilities.Google.Entities;
using Geoloocation.DB;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Geolocation.BL
{
    public static class DistanceRepository
    {
        private static IDB _db;

        static DistanceRepository()
        {
            _db = DbFactory.DB;
        }

        public static async Task<double> GetDistance(string source, string destination)
        {
            source = source.ToLower();
            destination = destination.ToLower();

            DistanceDbDto distance = null;

            try
            {
                distance = await GetDistanceFromDB(source, destination);

                if (distance != null)
                {
                    UpdateSearchCountAndPopularSearch(distance);
                    return distance.Distance;
                }
            } catch { }

            distance = await GetDistanceAndSaveToDB(source, destination);
            return distance?.Distance ?? -1;
        }

        private static async Task<DistanceDbDto> GetDistanceFromDB(string source, string destination)
            => await GetOneDirectionDistanceFromDB(source, destination) ?? await GetOneDirectionDistanceFromDB(destination, source);

        private static async Task<DistanceDbDto> GetOneDirectionDistanceFromDB(string source, string destination)
        {
            DistanceDbDto DistanceDbDto = await _db.Get<DistanceDbDto>(source, destination);
            return DistanceDbDto;
        }

        private static async Task<DistanceDbDto> GetDistanceAndSaveToDB(string source, string destination)
        {
            int? nullableDistance = null;

            if (source == destination)
            {
                nullableDistance = 0;
            }
            else
            {
                var getSourcePlaceTask = GetPlaceAndSaveToDbIfNotExists(source);
                var getDestPlaceTask = GetPlaceAndSaveToDbIfNotExists(destination);
                PlaceDbDto sourcePlace = await getSourcePlaceTask;
                PlaceDbDto destPlace = await getDestPlaceTask;
                DistanceMatrixResponseDto distanceMatrix = await DistanceMatrixApi.Distance(sourcePlace.GooglePlaceId, destPlace.GooglePlaceId);

                nullableDistance = distanceMatrix?.Rows?.FirstOrDefault()?.Elements?.FirstOrDefault()?.Distance?.Value;

                if (nullableDistance == null)
                {
                    return null;
                }
            }            

            var distance = new DistanceDbDto
            {
                Source = source,
                Destination = destination,
                Distance = MetersToKM((int)nullableDistance),
                SearchCount = 1,
            };

            try
            {
                _db.Insert(distance);
            } catch { }

            return distance;

            double MetersToKM(int meters) => meters / 1000.0;
        }

        private static async Task<PlaceDbDto> GetPlaceAndSaveToDbIfNotExists(string place)
        {
            PlaceDbDto placeDbDto = null;

            try
            {
                placeDbDto = await PlaceDbCache.GetPlace(place);

                if (placeDbDto != null)
                {
                    return placeDbDto;
                }
            } catch { }

            var autocompletePlace = await PlacesApi.AutoComplete(place);

            placeDbDto = new PlaceDbDto
            {
                PlaceName = place,
                GooglePlaceId = autocompletePlace.Predictions.ToList().FirstOrDefault()?.PlaceId,
            };

            try
            {
                if (placeDbDto.GooglePlaceId != null)
                {
                    PlaceDbCache.InsertPlace(placeDbDto);
                }
            } catch { }

            return placeDbDto;
        }        

        private static async Task UpdateSearchCountAndPopularSearch(DistanceDbDto distance)
        {
            distance.SearchCount += 1;

            await Task.WhenAll(
                UpdateSearchCountForDistance(distance),
                UpdatePopularSearch(distance));
        }

        private static async Task UpdateSearchCountForDistance(DistanceDbDto distance)
        {
            var propertiesToUpdate = new Dictionary<string, (object value, DbType type)>
            {
                [nameof(DistanceDbDto.SearchCount)] = (value: distance.SearchCount, type: DbType.Number),
            };

            await _db.Update<DistanceDbDto>(propertiesToUpdate, distance.Source, distance.Destination);
        }

        private static async Task UpdatePopularSearch(DistanceDbDto distance)
        {
            var populatSearch = await GetSavedSearchFromDB(SavedSearchesNames.POPULAR_SEARCH);
            int max = populatSearch?.SearchData?.SearchCount ?? 0;

            if (distance.SearchCount > max)
            {
                var popularSearch = new SearchDbDto
                {
                    SavedSearchName = SavedSearchesNames.POPULAR_SEARCH,
                    SearchData = new SearchDataDbDto
                    {
                        Source = distance.Source,
                        Destination = distance.Destination,
                        SearchCount = distance.SearchCount,
                    }
                };

                await _db.Update(popularSearch, SavedSearchesNames.POPULAR_SEARCH);
            }
        }

        public static async Task<(string source, string destination, int hits)> GetMostPopulatSearch()
        {
            SearchDbDto popularSearchProperty = await GetSavedSearchFromDB(SavedSearchesNames.POPULAR_SEARCH);
            var searchData = popularSearchProperty?.SearchData;
            
            if (searchData == null)
            {
                return (null, null, 0);
            }

            return (searchData.Source, searchData.Destination, searchData.SearchCount);
        }

        private static async Task<SearchDbDto> GetSavedSearchFromDB(string searchName)
            => await _db.Get<SearchDbDto>(searchName);

        public static async Task<int> InjectDistanceAndReturnHits(string source, string destination, double distance)
        {
            DistanceDbDto DistanceDbDto = await GetDistanceFromDB(source, destination);
            
            if (DistanceDbDto == null)
            {
                DistanceDbDto = new DistanceDbDto
                {
                    Source = source,
                    Destination = destination,
                };
            }

            DistanceDbDto.Distance = distance;
            UpdateDistance(DistanceDbDto);
            return DistanceDbDto?.SearchCount ?? 0;
        }

        private static async Task UpdateDistance(DistanceDbDto distance)
        {
            var propertiesToUpdate = new Dictionary<string, (object value, DbType type)>
            {
                [nameof(DistanceDbDto.Distance)] = (value: distance.Distance, type: DbType.Number),
            };

            await _db.Update<DistanceDbDto>(propertiesToUpdate, distance.Source, distance.Destination);
        }
    }
}
