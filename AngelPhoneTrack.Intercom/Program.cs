using Google.Cloud.TextToSpeech.V1;
using Microsoft.AspNetCore.SignalR.Client;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AngelPhoneTrack.Intercom
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var department = File.ReadAllText("department.txt");
            Console.WriteLine("Connecting as department: " + department);

            var connection = new HubConnectionBuilder()
                .WithUrl("https://localhost:7236/realtime?department=" + department.ToUpper())
                .Build();

            var client = new TextToSpeechClientBuilder
            {
                JsonCredentials = File.ReadAllText("angelphonetrackintercom.json")
            }.Build();

            connection.Closed += async (error) =>
            {
                await Task.Delay(new Random().Next(0, 5) * 1000);
                await connection.StartAsync();
            };

            connection.On<int, Priority, int, string, string, DateTimeOffset, string>("LotAssignmentsUpdate", 
                async (reassignment, priority, count, model, grade, expiration, lotNo) =>
            {
                Console.WriteLine("Received: " + reassignment + " for " + lotNo);
                string lotNoProcessed = string.Join("", lotNo.Select(x => (" " + x).Replace("0", "Zero"))).Trim();

                string text = $"Attention, {department} Department: Lot number: {lotNoProcessed}; "
                 + $"is being sent to you with {priority.ToString()} priority. This transfer will contain {reassignment} devices.";
                await PlayText(text);
            });

            connection.On<string, int, Priority, DateTimeOffset, string>("LotLate",
                async (lotNo, count, priority, expiration, name) =>
            {
                Console.WriteLine("Received late notification for " + lotNo);
                string lotNoProcessed = string.Join("", lotNo.Select(x => (" " + x).Replace("0", "Zero"))).Trim();
                string text = $"Attention, {department} Department: This is a reminder by {name} that, Lot number: {lotNoProcessed}; "
                 + $"is currently late. This lot is currently marked as {priority.ToString()} priority. Please prioritize this lot and complete it immediately.";
                
                await PlayText(text);
            });

            async Task PlayText(string text)
            {
                var request = new SynthesizeSpeechRequest()
                {
                    Input = new SynthesisInput()
                    {
                        Text = text
                    },
                    Voice = new VoiceSelectionParams()
                    {
                        LanguageCode = "en",
                        Name = "en-US-Wavenet-G",
                    },
                    AudioConfig = new AudioConfig()
                    {
                        AudioEncoding = AudioEncoding.Mp3,
                        SpeakingRate = 0.80
                    }
                };

                var res = await client.SynthesizeSpeechAsync(request);
                using (var file = File.Create("temp.mp3"))
                    res.AudioContent.WriteTo(file);

                using (var audioFile1 = new AudioFileReader("ping.mp3"))
                using (var audioFile = new AudioFileReader("temp.mp3"))
                using (var outputDevice = new WaveOutEvent())
                {
                    outputDevice.Init(audioFile1);
                    outputDevice.Play();
                    while (outputDevice.PlaybackState == PlaybackState.Playing)
                        await Task.Delay(1000);

                    outputDevice.Init(audioFile);
                    outputDevice.Play();

                    while (outputDevice.PlaybackState == PlaybackState.Playing)
                        await Task.Delay(1000);
                }

                File.Delete("temp.mp3");
            }

            connection.StartAsync().Wait();
            Thread.Sleep(-1);
        }
    }
}
