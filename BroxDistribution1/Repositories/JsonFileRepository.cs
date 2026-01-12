using System.Text.Json;

namespace BroxDistribution1.Repositories
{
    public class JsonFileRepository<T> where T : class
    {
        private readonly string _filePath;
        private static readonly SemaphoreSlim _fileLock = new SemaphoreSlim(1, 1);

        public JsonFileRepository(IWebHostEnvironment environment, string fileName)
        {
            var dataFolder = Path.Combine(environment.ContentRootPath, "App_Data");
            
            Console.WriteLine($"🔧 Initializing {typeof(T).Name} repository");
            Console.WriteLine($"🔧 Data folder: {dataFolder}");
            
            if (!Directory.Exists(dataFolder))
            {
                Directory.CreateDirectory(dataFolder);
                Console.WriteLine($"🔧 Created directory: {dataFolder}");
            }
            
            _filePath = Path.Combine(dataFolder, fileName);
            Console.WriteLine($"🔧 File path: {_filePath}");
            
            if (!File.Exists(_filePath))
            {
                File.WriteAllText(_filePath, "[]");
                Console.WriteLine($"🔧 Created empty JSON file: {fileName}");
            }
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            Console.WriteLine($"📖 Reading all {typeof(T).Name} records");
            
            await _fileLock.WaitAsync().ConfigureAwait(false);
            try
            {
                if (!File.Exists(_filePath))
                {
                    Console.WriteLine($"⚠️ File not found, returning empty list");
                    return new List<T>();
                }

                var json = await File.ReadAllTextAsync(_filePath).ConfigureAwait(false);
                Console.WriteLine($"📖 Read {json.Length} characters from file");
                
                if (string.IsNullOrWhiteSpace(json))
                {
                    Console.WriteLine($"⚠️ File is empty, returning empty list");
                    return new List<T>();
                }

                var items = JsonSerializer.Deserialize<List<T>>(json, new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                });
                
                Console.WriteLine($"✅ Deserialized {items?.Count ?? 0} items");
                return items ?? new List<T>();
            }
            finally
            {
                _fileLock.Release();
            }
        }

        public async Task<T> GetByIdAsync(int id)
        {
            var items = await GetAllAsync().ConfigureAwait(false);
            var idProperty = typeof(T).GetProperty("Id");
            return items.FirstOrDefault(item => (int)idProperty.GetValue(item) == id);
        }

        public async Task<T> AddAsync(T entity)
        {
            Console.WriteLine($"➕ Adding new {typeof(T).Name}");
            
            await _fileLock.WaitAsync().ConfigureAwait(false);
            try
            {
                Console.WriteLine($"🔒 Lock acquired for Add operation");
                
                // Read file directly without calling GetAllAsync
                List<T> items;
                if (File.Exists(_filePath))
                {
                    var json = await File.ReadAllTextAsync(_filePath).ConfigureAwait(false);
                    items = string.IsNullOrWhiteSpace(json) 
                        ? new List<T>() 
                        : JsonSerializer.Deserialize<List<T>>(json, new JsonSerializerOptions 
                        { 
                            PropertyNameCaseInsensitive = true 
                        }) ?? new List<T>();
                }
                else
                {
                    items = new List<T>();
                }
                
                Console.WriteLine($"📖 Found {items.Count} existing items");
                
                var idProperty = typeof(T).GetProperty("Id");
                int maxId = items.Any() ? items.Max(i => (int)idProperty.GetValue(i)) : 0;
                idProperty.SetValue(entity, maxId + 1);
                
                Console.WriteLine($"➕ Assigned ID: {maxId + 1}");
                
                items.Add(entity);
                
                var options = new JsonSerializerOptions 
                { 
                    WriteIndented = true,
                    PropertyNamingPolicy = null
                };
                var jsonToSave = JsonSerializer.Serialize(items, options);
                await File.WriteAllTextAsync(_filePath, jsonToSave).ConfigureAwait(false);
                
                Console.WriteLine($"✅ {typeof(T).Name} added successfully");
                return entity;
            }
            finally
            {
                _fileLock.Release();
                Console.WriteLine($"🔓 Lock released for Add operation");
            }
        }

        public async Task<T> UpdateAsync(T entity)
        {
            Console.WriteLine($"🔄 Updating {typeof(T).Name}");
            
            await _fileLock.WaitAsync().ConfigureAwait(false);
            try
            {
                List<T> items;
                if (File.Exists(_filePath))
                {
                    var json = await File.ReadAllTextAsync(_filePath).ConfigureAwait(false);
                    items = string.IsNullOrWhiteSpace(json) 
                        ? new List<T>() 
                        : JsonSerializer.Deserialize<List<T>>(json, new JsonSerializerOptions 
                        { 
                            PropertyNameCaseInsensitive = true 
                        }) ?? new List<T>();
                }
                else
                {
                    return entity;
                }
                
                var idProperty = typeof(T).GetProperty("Id");
                var entityId = (int)idProperty.GetValue(entity);
                
                var index = items.FindIndex(i => (int)idProperty.GetValue(i) == entityId);
                if (index >= 0)
                {
                    items[index] = entity;
                    
                    var options = new JsonSerializerOptions 
                    { 
                        WriteIndented = true,
                        PropertyNamingPolicy = null
                    };
                    var jsonToSave = JsonSerializer.Serialize(items, options);
                    await File.WriteAllTextAsync(_filePath, jsonToSave).ConfigureAwait(false);
                    
                    Console.WriteLine($"✅ {typeof(T).Name} updated successfully");
                }
                return entity;
            }
            finally
            {
                _fileLock.Release();
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            Console.WriteLine($"🗑️ Deleting {typeof(T).Name} with ID: {id}");
            
            await _fileLock.WaitAsync().ConfigureAwait(false);
            try
            {
                List<T> items;
                if (File.Exists(_filePath))
                {
                    var json = await File.ReadAllTextAsync(_filePath).ConfigureAwait(false);
                    items = string.IsNullOrWhiteSpace(json) 
                        ? new List<T>() 
                        : JsonSerializer.Deserialize<List<T>>(json, new JsonSerializerOptions 
                        { 
                            PropertyNameCaseInsensitive = true 
                        }) ?? new List<T>();
                }
                else
                {
                    return false;
                }
                
                var idProperty = typeof(T).GetProperty("Id");
                var item = items.FirstOrDefault(i => (int)idProperty.GetValue(i) == id);
                
                if (item != null)
                {
                    items.Remove(item);
                    
                    var options = new JsonSerializerOptions 
                    { 
                        WriteIndented = true,
                        PropertyNamingPolicy = null
                    };
                    var jsonToSave = JsonSerializer.Serialize(items, options);
                    await File.WriteAllTextAsync(_filePath, jsonToSave).ConfigureAwait(false);
                    
                    Console.WriteLine($"✅ {typeof(T).Name} deleted successfully");
                    return true;
                }
                return false;
            }
            finally
            {
                _fileLock.Release();
            }
        }
    }
}
