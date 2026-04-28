using PhotonPlayer = Photon.Realtime.Player;

public static class PhotonPlayerExtensions
{
    // 커스텀 프로퍼티 읽기 (EProperties 키) - 키가 없으면 defaultValue 반환
    public static T GetCustomProperty<T>(this PhotonPlayer player, EProperties key, T defaultValue = default)
    {
        string keyStr = key.ToString();
        if (player.CustomProperties.ContainsKey(keyStr) && player.CustomProperties[keyStr] != null)
        {
            return (T)player.CustomProperties[keyStr];
        }
        return defaultValue;
    }

    // 커스텀 프로퍼티 읽기 (string 키) - 키가 없으면 defaultValue 반환
    public static T GetCustomProperty<T>(this PhotonPlayer player, string key, T defaultValue = default)
    {
        if (player.CustomProperties.ContainsKey(key) && player.CustomProperties[key] != null)
        {
            return (T)player.CustomProperties[key];
        }
        return defaultValue;
    }
}
