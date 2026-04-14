using UnityEngine;
using FishNet.Managing;
using FishNet.Transporting.Tugboat;

[RequireComponent(typeof(NetworkManager))]
public class NetworkCommandLineArgs : MonoBehaviour
{
    private NetworkManager _networkManager;

    private void Start()
    {
        _networkManager = GetComponent<NetworkManager>();

        // 1. Парсинг аргументов (этот код всегда будет в сборке)
        string bindIp = GetBindIpFromCommandLine();
        ushort? port = GetPortFromCommandLine();

        // 2. Применяем настройки к транспорту
        var tugboat = _networkManager.TransportManager.Transport as Tugboat;
        if (tugboat != null)
        {
            if (!string.IsNullOrEmpty(bindIp))
            {
                tugboat.SetServerBindAddress(bindIp, FishNet.Transporting.IPAddressType.IPv4);
                Debug.Log($"Bind IP set to {bindIp}");
            }
            if (port.HasValue)
            {
                tugboat.SetPort(port.Value);
                Debug.Log($"Port set to {port}");
            }
        }

        // 3. Запускаем сервер только в серверной сборке
        #if UNITY_SERVER
        // Небольшая задержка, чтобы настройщик транспорта успел отработать
            if (!Application.isEditor)
            {
                //Debug.Log("Unity_Server");
                Invoke(nameof(StartServer), 0.1f);
            }
        #endif
    }

    private void StartServer()
    {
        _networkManager.ServerManager.StartConnection();
    }

    private string GetBindIpFromCommandLine()
    {
        string[] args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "-bindIp" && i + 1 < args.Length)
                return args[i + 1];
        }
        return null;
    }

    private ushort? GetPortFromCommandLine()
    {
        string[] args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "-port" && i + 1 < args.Length)
            {
                if (ushort.TryParse(args[i + 1], out ushort port))
                    return port;
            }
        }
        return null;
    }
}
