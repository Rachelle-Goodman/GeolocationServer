using Geolocation.Utilities.Aws.DynamoDB;
using Geolocation.Utilities.Aws.DynamoDB.Entities;
using Geolocation.Utilities.Google;
using Geolocation.Utilities.Google.Entities;
using System.Linq;
using System.Threading.Tasks;

namespace Geolocation.BL
{
    public static class DistanceRepository
    {
        public static async Task<double> GetDistance(string source, string destination)
        {
            source = source.ToLower();
            destination = destination.ToLower();

            DistanceDdbDto distance = null;

            try
            {
                distance = await GetDistanceFromDB(source, destination) ?? await GetDistanceFromDB(destination, source);

                if (distance != null)
                {
                    return distance.Distance;
                }
            } catch { }

            distance = await GetDistanceAndSaveToDB(source, destination);
            return distance.Distance;
        }

        private static async Task<DistanceDdbDto> GetDistanceFromDB(string source, string destination)
            => await DynamoDbAdapter.Get<DistanceDdbDto>(source, destination);

        private static async Task<DistanceDdbDto> GetDistanceAndSaveToDB(string source, string destination)
        {
            var getSourcePlaceTask = GetPlaceAndSaveToDbIfNotExists(source);
            var getDestPlaceTask = GetPlaceAndSaveToDbIfNotExists(destination);
            PlaceDdbDto sourcePlace = await getSourcePlaceTask;
            PlaceDdbDto destPlace = await getDestPlaceTask;
            DistanceMatrixResponseDto distanceMatrix = await DistanceMatrixApi.Distance(sourcePlace.GooglePlaceId, destPlace.GooglePlaceId);

            int? nullableDistance = distanceMatrix.Rows?.FirstOrDefault()?.Elements.FirstOrDefault()?.Distance.Value;            

            if (nullableDistance == null)
            {
                return null;
            }

            var distance = new DistanceDdbDto
            {
                Source = source,
                Destination = destination,
                Distance = MetersToKM((int)nullableDistance),
            };

            try
            {
                DynamoDbAdapter.Insert(distance);
            } catch { }

            return distance;

            double MetersToKM(int meters) => meters / 1000.0;
        }

        private static async Task<PlaceDdbDto> GetPlaceAndSaveToDbIfNotExists(string place)
        {
            PlaceDdbDto placeDdbDto = null;

            try
            {
                placeDdbDto = await GetPlaceFromDB(place);

                if (placeDdbDto != null)
                {
                    return placeDdbDto;
                }
            } catch { }

            var autocompletePlace = await PlacesApi.AutoComplete(place);

            placeDdbDto = new PlaceDdbDto
            {
                PlaceName = place,
                GooglePlaceId = autocompletePlace.Predictions.ToList().First().PlaceId,
            };

            try
            {
                DynamoDbAdapter.Insert(placeDdbDto);
            } catch { }

            return placeDdbDto;
        }

        private static async Task<PlaceDdbDto> GetPlaceFromDB(string place)
            => await DynamoDbAdapter.Get<PlaceDdbDto>(place);

    }
}
