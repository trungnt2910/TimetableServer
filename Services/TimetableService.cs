using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TimetableServer.Models;

namespace TimetableServer.Services
{
    public class TimetableService
    {
        const int limit = 10;

        private readonly IMongoCollection<Timetable> _timetables;
        private readonly Queue<(string, DateTime)> _keys = new Queue<(string, DateTime)>();
        private readonly Dictionary<string, string> _timetableValues = new Dictionary<string, string>();

        public TimetableService(ITimetableDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _timetables = database.GetCollection<Timetable>(settings.CollectionName);
        }

        public List<Timetable> Get() =>
            _timetables.Find(t => true).ToList();

        public TimetableLocationResponse Get(string id, string password, string domainName)
        {
            var timetable = _timetables.Find(t => t.Id == id && t.Password == password).FirstOrDefault();
            if (timetable == null)
            {
                return null;
            }
            if (string.IsNullOrEmpty(timetable.SHA512))
            {
                timetable.SHA512 = CalculateSha512(timetable.Content);
                _timetables.ReplaceOne(t => t.Id == id && t.Password == password, timetable);
            }
            return new TimetableLocationResponse()
            {
                MD5 = timetable.MD5,
                SHA512 = timetable.SHA512,
                // To-Do: Generate link.
                Location = GenerateTemporaryLocation(timetable, domainName)
            };
        }

        public bool Update(string id, string updatePassword, string data)
        {
            var targetTimetable = _timetables.Find(table => table.Id == id && table.UpdatePassword == updatePassword).FirstOrDefault();
            if (targetTimetable == null)
            {
                return false;
            }
            _timetables.ReplaceOne(table => table.Id == id && table.UpdatePassword == updatePassword, new Timetable()
            {
                Id = id,
                Password = targetTimetable.Password,
                UpdatePassword = targetTimetable.UpdatePassword,
                Content = data,
                MD5 = CalculateMd5(data),
                SHA512 = CalculateSha512(data),
                IpAddress = targetTimetable.IpAddress
            });
            return true;
        }

        public string GetTimetable(string uuid)
        {
            UpdateKeys();
            _timetableValues.TryGetValue(uuid, out string result);
            return result;
        }

        public string CreateTimetable(System.Net.IPAddress address, string password, string updatePassword, string data)
        {
            var count = _timetables.Find((t) => t.IpAddress == address.ToString()).CountDocuments();
            if (count >= limit)
            {
                throw new InvalidOperationException("User limit exceeded.");
            }
            var id = ObjectId.GenerateNewId();
            _timetables.InsertOne(new Timetable()
            {
                Id = id.ToString(),
                Password = password,
                UpdatePassword = updatePassword,
                Content = data,
                MD5 = CalculateMd5(data),
                SHA512 = CalculateSha512(data),
                IpAddress = address.ToString()
            });
            return id.ToString();
        }

        public bool DeleteTimetable(string id, string updatePassword)
        {
            var targetTimetable = _timetables.Find(table => table.Id == id && table.UpdatePassword == updatePassword).FirstOrDefault();
            if (targetTimetable == null)
            {
                return false;
            }
            return _timetables.DeleteOne(table => table.Id == id && table.UpdatePassword == updatePassword).DeletedCount > 0;
        }

        private string GenerateTemporaryLocation(Timetable table, string domainName)
        {
            UpdateKeys();
            lock (_keys)
            {
                var currentDate = DateTime.Now;
                var uuid = Guid.NewGuid().ToString();
                _timetableValues.Add(uuid, table.Content);
                _keys.Enqueue((uuid, currentDate));
                return $"{domainName}/api/timetable/getTimetable?uuid={uuid}";
            }
        }

        private void UpdateKeys()
        {
            lock (_keys)
            {
                var currentDate = DateTime.Now;
                var span = TimeSpan.FromMinutes(1);

                while ((_keys.Count > 0) && (_keys.Peek().Item2 - currentDate >= span))
                {
                    var key = _keys.Dequeue();
                    _timetableValues.Remove(key.Item1);
                }
            }
        }

        private static string CalculateMd5(string data)
        {
            using var md5 = MD5.Create();
            var encoding = new UTF8Encoding();
            var hash = md5.ComputeHash(encoding.GetBytes(data));
            return BitConverter.ToString(hash).Replace("-", "").ToUpperInvariant();
        }

        private static string CalculateSha512(string data)
        {
            using var sha512 = SHA512.Create();
            var encoding = new UTF8Encoding();
            var hash = sha512.ComputeHash(encoding.GetBytes(data));
            return BitConverter.ToString(hash).Replace("-", "").ToUpperInvariant();
        }

        //public Book Get(string id) =>
        //    _books.Find<Book>(book => book.Id == id).FirstOrDefault();

        //public Book Create(Book book)
        //{
        //    _books.InsertOne(book);
        //    return book;
        //}

        //public void Update(string id, Book bookIn) =>
        //    _books.ReplaceOne(book => book.Id == id, bookIn);

        //public void Remove(Book bookIn) =>
        //    _books.DeleteOne(book => book.Id == bookIn.Id);

        //public void Remove(string id) =>
        //    _books.DeleteOne(book => book.Id == id);
    }
}
