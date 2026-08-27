/*
*	<copyright file="IdentityService">
*	</copyright>
* 	<author>Marco Macedo</author>
*	<contact>a26874@alunos.ipca.pt</contact>
*   <date>2025 12/14/2025 12:47:20 PM</date>
*	<description></description>
**/

using Backend_sec_dev.Application.DTO_s;
using Backend_sec_dev.Application.Interfaces;
using Backend_sec_dev.Domain.Entities;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace Backend_sec_dev.Application.Services
{
    public class PerformanceService : IPerformanceService
    {
        protected readonly IHttpContextAccessor http;
        protected readonly IDatabaseRepository databaseRepository;

        protected readonly Dictionary<string, Type> allowedBulkInsertTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            {"User", typeof(User)}
        };

        public PerformanceService(IHttpContextAccessor httpContext, IDatabaseRepository databaseRepository)
        {
            this.http = httpContext;
            this.databaseRepository = databaseRepository;
        }

        #region public

        /// <summary>
        /// Logins
        /// </summary>
        /// <returns></returns>
        public async Task<ApiResponse<bool>> UploadCSVFile(UploadCSVFileDto base64CsvFile)
        {
            byte[] fileBytes = Convert.FromBase64String(base64CsvFile.File);

            string csv = Encoding.UTF8.GetString(fileBytes);

            string[] lines = csv.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            List<Customers> customers = new List<Customers>();

            var table = new DataTable();
            table.Columns.Add("CustomerId", typeof(Guid));
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("Email", typeof(string));
            table.Columns.Add("Phone", typeof(string));
            table.Columns.Add("Country", typeof(string));
            table.Columns.Add("BirthDate", typeof(DateOnly));


            foreach (string line in lines.Skip(1))
            {
                string[] columns = line.Split(',');

                var customer = new Customers
                {
                    CustomerId = Guid.Parse(columns[0].Trim()),
                    Name = columns[1].Trim(),
                    Email = columns[2].Trim(),
                    Phone = columns[3].Trim(),
                    Country = columns[4].Trim(),
                };

                table.Rows.Add(
                   Guid.Parse(columns[0].Trim()),
                   columns[1].Trim(),
                   columns[2].Trim(),
                   columns[3].Trim(),
                   columns[4].Trim(),
                   new DateOnly()
               );

                customers.Add(customer);
            }

            Stopwatch firstStopwatch = new Stopwatch();
            Stopwatch secondStopwatch = new Stopwatch();
            Stopwatch thirdStopwatch = new Stopwatch();
            Stopwatch fourthStopwatch = new Stopwatch();
            Stopwatch fifthStopwatch = new Stopwatch();

            //databaseRepository.ExecuteRawSql("TRUNCATE TABLE Customers");

            //firstStopwatch.Start();
            //this.databaseRepository.AddRange(customers);
            //await this.databaseRepository.SaveChanges();
            //firstStopwatch.Stop();

            //databaseRepository.ClearChangeTracker();
            //databaseRepository.ExecuteRawSql("TRUNCATE TABLE Customers");


            //secondStopwatch.Start();
            //foreach (Customers item in customers)
            //{
            //    this.databaseRepository.Add<Customers>(item);
            //}
            //await this.databaseRepository.SaveChanges();
            //secondStopwatch.Stop();

            //databaseRepository.ClearChangeTracker();
            //databaseRepository.ExecuteRawSql("TRUNCATE TABLE Customers");

            //thirdStopwatch.Start();
            //int take = 0;
            //while (take < customers.Count)
            //{
            //    var taked = customers.Skip(take).Take(1000).ToList();
            //    this.databaseRepository.AddRange(taked);
            //    await this.databaseRepository.SaveChanges();
            //    take += 1000;
            //}
            //thirdStopwatch.Stop();

            //databaseRepository.ClearChangeTracker();
            //databaseRepository.ExecuteRawSql("TRUNCATE TABLE Customers");

            //fourthStopwatch.Start();
            //take = 0;
            //List<string> chunkRecords = new List<string>();
            //while (take < customers.Count)
            //{

            //    List<string> tempChunk = new List<string>();
            //    string headerInsert = "INSERT INTO CUSTOMERS (CustomerId, Name, Email, Phone, Country, BirthDate) VALUES ";
            //    var taked = customers.Skip(take).Take(1000).ToList();

            //    for (int i = 0; i < taked.Count; i++)
            //    {
            //        string record = $"('{taked[i].CustomerId}', '{taked[i].Name}', '{taked[i].Email}', '{taked[i].Phone}', '{taked[i].Country}', '{new DateOnly()}')";
            //        tempChunk.Add(record);
            //    }

            //    take += 1000;
            //    chunkRecords.Add(headerInsert + string.Join(", ", tempChunk));
            //}
            //foreach (string item in chunkRecords)
            //{
            //    this.databaseRepository.ExecuteRawSql(item);
            //}
            //fourthStopwatch.Stop();
            databaseRepository.ExecuteRawSql("TRUNCATE TABLE Customers");

            for (int times = 0; times < 10; times++)
            {
                databaseRepository.ExecuteRawSql("TRUNCATE TABLE Customers");
                fifthStopwatch.Start();
                using var connection = new SqlConnection("Server=localhost;Database=BACKEND_DEV;Trusted_Connection=True;TrustServerCertificate=True;");
                await connection.OpenAsync();

                using var bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.TableLock, null)
                {
                    DestinationTableName = "dbo.Customers",
                    BatchSize = 20000,
                    BulkCopyTimeout = 60
                };

                bulkCopy.ColumnMappings.Add("CustomerId", "CustomerId");
                bulkCopy.ColumnMappings.Add("Name", "Name");
                bulkCopy.ColumnMappings.Add("Email", "Email");
                bulkCopy.ColumnMappings.Add("Phone", "Phone");
                bulkCopy.ColumnMappings.Add("Country", "Country");
                bulkCopy.ColumnMappings.Add("BirthDate", "BirthDate");

                await bulkCopy.WriteToServerAsync(table);

                fifthStopwatch.Stop();
                Console.WriteLine($"SqlBulkCopy run {times}: {fifthStopwatch.Elapsed.Seconds}s {fifthStopwatch.Elapsed.Milliseconds}ms");
                fifthStopwatch.Restart();
            }
            //databaseRepository.ExecuteRawSql("TRUNCATE TABLE Customers");



            Console.WriteLine($"AddRange: {firstStopwatch.Elapsed.Seconds}s {firstStopwatch.Elapsed.Milliseconds}ms");
            Console.WriteLine($"ForEach: {secondStopwatch.Elapsed.Seconds}s {secondStopwatch.Elapsed.Milliseconds}ms");
            Console.WriteLine($"Batching AddRange: {thirdStopwatch.Elapsed.Seconds}s {thirdStopwatch.Elapsed.Milliseconds}ms");
            Console.WriteLine($"Bulk Insert: {fourthStopwatch.Elapsed.Seconds}s {fourthStopwatch.Elapsed.Milliseconds}ms");
            Console.WriteLine($"SqlBulkCopy: {fifthStopwatch.Elapsed.Seconds}s {fifthStopwatch.Elapsed.Milliseconds}ms");



            return ApiResponse<bool>.Ok(true, System.Net.HttpStatusCode.Accepted);
        }


        /// <summary>
        /// If for some reason theres is the need to post a huge object, this handles it
        /// Should be used when inserting through ORM is very inefficient.
        /// TO USE:
        /// If its needed to send a new class, add it to the allowedBulkInsertTypes dictionary, with the key being the name of the class and the value being the type of the class.
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        /// <remarks>
        /// The only problem with this method is that it bypasses all EF behaviours, like changetracking, validations, interceptors etc
        /// Only the database-level validation and constraints still take palce.
        /// We can use a custom validation of the type we are processing.
        /// </remarks>
        public async Task<ApiResponse<bool>> ProcessBulkInsert(GenericPostDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Type)) return ApiResponse<bool>.Fail(System.Net.HttpStatusCode.BadRequest, "Object is null");

            using var connection = this.databaseRepository.GetSqlConnection()!;

            if (connection.State != ConnectionState.Open)
                await connection.OpenAsync();

            //When creating the transaction we already begin the transaction (if it is not yet opened)
            SqlTransaction? sqlTransaction = this.databaseRepository.CreateSqlTransaction();

            if (sqlTransaction == null) throw new Exception("Couldnt create a sql transaction");

            try
            {
                //Idk if its the best thing to do, but since its a json if it contains [ it must be a array/collection.
                bool isArrayOrCollection = dto.data.RootElement.GetRawText().StartsWith('[') ? true : false;

                object? deserializedObject = GetDeserializedObject(dto, isArrayOrCollection);
                if (deserializedObject == null) return ApiResponse<bool>.Fail(System.Net.HttpStatusCode.BadRequest, "Deserialized object is null");

                List<Tuple<SqlBulkCopy, DataTable>> bulkCopies = new List<Tuple<SqlBulkCopy, DataTable>>();

                bool valid = isArrayOrCollection ? HandleArrayOrCollectionPayload(deserializedObject, connection, sqlTransaction, bulkCopies) : HandleSingleObjectPayload(deserializedObject, connection, sqlTransaction, bulkCopies);

                if (!valid) return ApiResponse<bool>.Fail(System.Net.HttpStatusCode.BadRequest, isArrayOrCollection ? "Invalid payload for array/collection request" : "Invalid payload for single request");

                int count = 0;

                foreach (Tuple<SqlBulkCopy, DataTable> tuple in bulkCopies)
                {
                    if (count == 1)
                    {
                        throw new Exception("Test exception to check rollback");
                    }
                    await tuple.Item1.WriteToServerAsync(tuple.Item2);
                    count++;

                }
                await sqlTransaction.CommitAsync();
                await connection.CloseAsync();
            }
            catch (Exception)
            {
                await sqlTransaction.RollbackAsync();
                throw;
            }

            return ApiResponse<bool>.Ok(true, System.Net.HttpStatusCode.Accepted);

        }

        private bool HandleArrayOrCollectionPayload(object? deserializedObject, SqlConnection connection, SqlTransaction transaction, List<Tuple<SqlBulkCopy, DataTable>> bulkCopies)
        {
            if (deserializedObject == null) return false;

            Type? listType = GetListType(deserializedObject);
            if (listType == null) return false;
            List<string> properties = listType.GetProperties().Select(t => t.Name).ToList();

            properties = listType.GetProperties().Select(t => t.Name).ToList();

            // Since its a list we have to convert the object into a list
            List<object> list = ConvertDataPayloadToList(deserializedObject);

            Tuple<SqlBulkCopy, DataTable>? mainBulkCopy = CreateBulkCopy(connection, transaction, deserializedObject, listType.Name);

            if (mainBulkCopy != null) bulkCopies.Add(mainBulkCopy);

            List<object> nestedObjects = new();
            string nestedTableName = string.Empty;

            foreach (object obj in list)
            {
                nestedTableName = string.Empty;
                foreach (string item in properties)
                {
                    PropertyInfo? currentItemPropertyInfo = obj.GetType().GetProperty(item);
                    Type? currentItemType = currentItemPropertyInfo?.PropertyType;

                    if (currentItemType != null && currentItemPropertyInfo != null && (IsEnumerableType(currentItemType) || IsCollectionType(currentItemType) || IsArrayType(currentItemType)))
                    {
                        nestedTableName = currentItemPropertyInfo.Name;

                        object? nestedValue = currentItemPropertyInfo.GetValue(obj);

                        if (nestedValue != null)
                        {
                            List<object> items = ConvertDataPayloadToList(nestedValue);
                            nestedObjects.AddRange(items);
                        }

                    }
                }
            }

            if (nestedObjects != null && nestedObjects.Count > 0)
            {
                Tuple<SqlBulkCopy, DataTable>? newCopy = CreateBulkCopy(connection, transaction, nestedObjects, nestedTableName);

                if (newCopy != null) bulkCopies.Add(newCopy);
            }

            return true;
        }

        private bool HandleSingleObjectPayload(object? deserializedObject, SqlConnection connection, SqlTransaction transaction, List<Tuple<SqlBulkCopy, DataTable>> bulkCopies)
        {
            if (deserializedObject == null) return false;
            List<string> properties = deserializedObject.GetType().GetProperties().Select(t => t.Name).ToList();

            Tuple<SqlBulkCopy, DataTable>? mainBulkCopy = CreateBulkCopy(connection, transaction, deserializedObject, deserializedObject.GetType().Name);
            if (mainBulkCopy != null)
            {
                bulkCopies.Add(mainBulkCopy);
            }

            foreach (string item in properties)
            {
                PropertyInfo? currentItemPropertyInfo = deserializedObject.GetType().GetProperty(item);
                Type? currentItemType = currentItemPropertyInfo?.PropertyType;

                if (currentItemType != null && currentItemPropertyInfo != null && (IsEnumerableType(currentItemType) || IsCollectionType(currentItemType) || IsArrayType(currentItemType)))
                {
                    Tuple<SqlBulkCopy, DataTable>? newCopy = CreateBulkCopy(connection, transaction, currentItemPropertyInfo.GetValue(deserializedObject), currentItemPropertyInfo.Name);
                    if (newCopy != null)
                    {
                        bulkCopies.Add(newCopy);
                    }
                }
            }

            return true;
        }

        private List<object> ConvertDataPayloadToList(object? deserializedObject)
        {
            if (deserializedObject == null) return new List<object>();
            List<object> list = ((System.Collections.IEnumerable)deserializedObject).Cast<object>().ToList();
            return list;
        }

        private object? GetDeserializedObject(GenericPostDto dto, bool isArrayOrCollection)
        {

            Type? wantedType = GetTypeFromGenericPostDto(dto.Type);

            if (wantedType == null) return ApiResponse<bool>.Fail(System.Net.HttpStatusCode.BadRequest, "Type not allowed");

            //Not sure if its the best way to detect that the coming object is a array/collection but for now it works

            object? deserializedObject = null;
            if (isArrayOrCollection)
            {
                // Its the way to get the generic type of List<T> where T is the wantedType, so we can deserialize the json to a List of the wantedType
                Type typeList = typeof(List<>).MakeGenericType(wantedType);
                deserializedObject = JsonSerializer.Deserialize(dto.data.RootElement.GetRawText(), typeof(List<>).MakeGenericType(wantedType), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            else
            {
                // Acceps a plain object
                deserializedObject = JsonSerializer.Deserialize(dto.data.RootElement.GetRawText(), wantedType, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }

            return deserializedObject;
        }

        private Type? GetTypeFromGenericPostDto(string genericPostDtoType)
        {
            Type? wantedType = null;
            this.allowedBulkInsertTypes.TryGetValue(genericPostDtoType, out wantedType);
            return wantedType;
        }

        private Tuple<SqlBulkCopy, DataTable>? CreateBulkCopy(SqlConnection connection, SqlTransaction transaction, object? item, string tableName)
        {
            if (item == null || tableName == null) return null;

            List<PropertyInfo>? props = GetPropertyInfos(item);

            if (props == null || props.Count == 0) return null;

            DataTable? table = CreateDataTable(item, props);

            if (table == null) return null;

            string tablePrefix = "dbo.";

            SqlBulkCopy bulkCopy = new(connection, SqlBulkCopyOptions.TableLock, transaction)
            {
                DestinationTableName = tablePrefix + tableName,
                BatchSize = 20000,
                BulkCopyTimeout = 60
            };

            foreach (PropertyInfo prop in props)
                bulkCopy.ColumnMappings.Add(prop.Name, prop.Name);

            return new Tuple<SqlBulkCopy, DataTable>(bulkCopy, table);
        }

        private DataTable? CreateDataTable(object item, List<PropertyInfo> props)
        {
            DataTable table = new DataTable();

            foreach (PropertyInfo prop in props)
            {
                Type typeForColumn = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                table.Columns.Add(prop.Name, typeForColumn);
            }
            bool isArrayOrCollection = IsEnumerableType(item.GetType()) || IsCollectionType(item.GetType()) || IsArrayType(item.GetType());

            if (isArrayOrCollection)
            {
                List<object> list = ConvertDataPayloadToList(item);

                foreach (object obj in list)
                {
                    DataRow row = table.NewRow();

                    foreach (PropertyInfo prop in props)
                    {
                        row[prop.Name] = prop.GetValue(obj) ?? DBNull.Value;
                    }

                    table.Rows.Add(row);
                }
            }
            else
            {
                DataRow row = table.NewRow();
                foreach (PropertyInfo prop in props)
                {
                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                }
                table.Rows.Add(row);
            }
            return table;
        }

        private List<PropertyInfo>? GetPropertyInfos(object? item)
        {
            if (item == null) return null;

            Type type = item.GetType();

            Type? itemType;

            if (IsEnumerableType(type) || IsCollectionType(type) || IsArrayType(type))
            {
                if (item is System.Collections.IEnumerable enumerable)
                {
                    object? firstItem = enumerable.Cast<object?>().FirstOrDefault();

                    if (firstItem == null) return null;

                    itemType = firstItem.GetType();
                }
                else
                {
                    itemType = GetListType(item);
                }
            }
            else
            {
                itemType = type;
            }

            if (itemType == null) return null;

            return itemType.GetProperties().Where(p => !IsEnumerableType(p.PropertyType) && !IsCollectionType(p.PropertyType) && !IsArrayType(p.PropertyType)).ToList();
        }



        private Type? GetListType(object obj)
        {
            return obj.GetType().IsArray ? obj.GetType().UnderlyingSystemType.GetElementType() : obj.GetType().UnderlyingSystemType.GetGenericArguments()[0];
        }

        private bool IsEnumerableType(Type type)
        {
            return typeof(System.Collections.IEnumerable).IsAssignableFrom(type) && type != typeof(string);
        }
        private bool IsCollectionType(Type type)
        {
            return typeof(System.Collections.ICollection).IsAssignableFrom(type) && type != typeof(string);
        }
        private bool IsArrayType(Type type)
        {
            return type.IsArray;
        }
        #endregion

    }
}