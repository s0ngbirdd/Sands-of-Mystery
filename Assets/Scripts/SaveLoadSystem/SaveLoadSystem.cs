using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class SaveLoadSystem : MonoBehaviour
{
    public static SaveLoadSystem Instance;

    private string _savePath;
    private string _savePath1;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        _savePath = Application.persistentDataPath + "/save.dat";
        _savePath1 = Application.persistentDataPath + "/save1.dat";
    }

    public void SaveGame(int score)
    {
        FileStream stream = File.Create(_savePath);
        BinaryFormatter formatter = new BinaryFormatter();
        formatter.Serialize(stream, score);
        stream.Close();
    }
    
    public void SaveGame1(int currency)
    {
        FileStream stream = File.Create(_savePath1);
        BinaryFormatter formatter = new BinaryFormatter();
        formatter.Serialize(stream, currency);
        stream.Close();
    }

    public int LoadGame()
    {
        if (File.Exists(_savePath))
        {
            FileStream stream = File.Open(_savePath, FileMode.Open);
            BinaryFormatter formatter = new BinaryFormatter();
            int score = (int)formatter.Deserialize(stream);
            stream.Close();
            return score;
        }
        else
        {
            return 0;
        }
    }
    
    public int LoadGame1()
    {
        if (File.Exists(_savePath1))
        {
            FileStream stream = File.Open(_savePath1, FileMode.Open);
            BinaryFormatter formatter = new BinaryFormatter();
            int currency = (int)formatter.Deserialize(stream);
            stream.Close();
            return currency;
        }
        else
        {
            return 0;
        }
    }
}
