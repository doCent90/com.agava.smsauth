using SmsAuthAPI.Utility;
using Agava.Wink.Samples;

namespace Agava.Wink
{
    public class SaveLoadService
    {
        public GameData Data { get; set; }

        public void Save()
        {
            SaveLoadCloudDataService.SaveData(Data);
        }

        public async void Load()
        {
            Data = await SaveLoadCloudDataService.LoadData<GameData>();
        }
    }
}
