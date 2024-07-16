using System;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class NetworkAudioStream : MonoBehaviour
{
    public string ipAddress = "192.168.1.137";
    public int port1 = 12345;
    public int port2 = 12346;

    public GameObject audioSourceObject1;
    public GameObject audioSourceObject2;

    private TcpClient audioClient1;
    private TcpClient audioClient2;
    private NetworkStream audioStream1;
    private NetworkStream audioStream2;
    private Thread audioThread1;
    private Thread audioThread2;
    private bool isRunning = false;

    private const int sampleRate = 44100;
    private const int bufferSize = 44100;

    private void Start()
    {
        StartAudioStreaming();
    }

    private void OnDestroy()
    {
        StopAudioStreaming();
    }

    public void StartAudioStreaming()
    {
        isRunning = true;

        audioThread1 = new Thread(() => ReceiveAudio(port1, audioSourceObject1));
        audioThread1.Start();

        audioThread2 = new Thread(() => ReceiveAudio(port2, audioSourceObject2));
        audioThread2.Start();
    }

    public void StopAudioStreaming()
    {
        isRunning = false;

        audioThread1?.Abort();
        audioThread2?.Abort();

        audioClient1?.Close();
        audioClient2?.Close();
    }

    private void ReceiveAudio(int port, GameObject audioSourceObject)
    {
        TcpClient audioClient = new TcpClient(ipAddress, port);
        NetworkStream audioStream = audioClient.GetStream();

        float[] audioBuffer = new float[bufferSize];
        int bytesRead;
        byte[] buffer = new byte[bufferSize * sizeof(short)];

        while (isRunning)
        {
            bytesRead = audioStream.Read(buffer, 0, buffer.Length);
            if (bytesRead > 0)
            {
                for (int i = 0; i < bytesRead / sizeof(short); i++)
                {
                    audioBuffer[i] = BitConverter.ToInt16(buffer, i * sizeof(short)) / 32768.0f;
                }

                AudioClip clip = AudioClip.Create("AudioStream", bufferSize, 1, sampleRate, false);
                clip.SetData(audioBuffer, 0);
                AudioSource audioSource = audioSourceObject.GetComponent<AudioSource>();
                audioSource.clip = clip;
                audioSource.Play();
            }
        }

        audioClient.Close();
    }
}
