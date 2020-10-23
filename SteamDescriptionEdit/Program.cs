using Steamworks;
using System;
using System.Collections.Generic;
using System.IO;

namespace SteamUpdateTool
{
    class Program
    {
        private const int rimworldId = 294100;
        private static uint workshopId;
        private static Steamworks.Ugc.Item workshopItem;

        static async System.Threading.Tasks.Task Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine($"No workshop-id defined.");
                return;
            }

            try
            {
                workshopId = Convert.ToUInt32(args[0]);
            }
            catch (Exception)
            {
                Console.WriteLine($"{args[0]} is not a valid workshopId");
                return;
            }

            try
            {

                SteamClient.Init(rimworldId, true);
                //Console.WriteLine($"Initiated steam-client.");
            }
            catch (Exception exception)
            {

                Console.WriteLine($"Could not connect to steam.\n{exception}");
                return;
            }

            if (args.Length == 1)
            {
                await LoadModInfoAsync();

                Console.WriteLine(workshopItem.Title);
                Console.WriteLine(workshopItem.Description);
                Console.SetOut(TextWriter.Null);
                SteamClient.Shutdown();
                return;
            }

            var operation = args[1];
            if (args.Length == 2)
            {

                Console.WriteLine($"Missing parameters for operation {operation}");
                Console.SetOut(TextWriter.Null);
                SteamClient.Shutdown();
                return;
            }
            var validOperations = new List<string> { "REPLACE", "SET", "SYNC" };
            if (!validOperations.Contains(operation))
            {

                Console.WriteLine($"{operation} is not a valid operation. \nValid values are: {string.Join(",", validOperations)}");
                Console.SetOut(TextWriter.Null);
                SteamClient.Shutdown();
                return;
            }
            switch (operation)
            {
                case "REPLACE":
                    if (args.Length != 4)
                    {

                        Console.WriteLine($"{operation} demands two arguments, a searchstring and a replacestring.");
                        Console.SetOut(TextWriter.Null);
                        SteamClient.Shutdown();
                        return;
                    }
                    var searchString = args[2];
                    var replaceString = args[3];
                    await LoadModInfoAsync();
                    if (!workshopItem.Description.Contains(searchString))
                    {

                        Console.WriteLine($"{workshopItem.Title} description does not contain {searchString}, skipping update");
                        Console.SetOut(TextWriter.Null);
                        SteamClient.Shutdown();
                        return;
                    }
                    var updatedDescription = workshopItem.Description.Replace(searchString, replaceString);
                    await SetModDescriptionAsync(updatedDescription);
                    break;
                case "SET":
                    if (args.Length != 3)
                    {

                        Console.WriteLine($"{operation} demands one argument, the updated descriptionstring");
                        Console.SetOut(TextWriter.Null);
                        SteamClient.Shutdown();
                        return;
                    }
                    await LoadModInfoAsync();
                    await SetModDescriptionAsync(args[2]);
                    break;
                case "SYNC":
                    var validArguments = new List<string> { "REMOTE", "LOCAL" };
                    if (args.Length != 3 || !validArguments.Contains(args[2]))
                    {

                        Console.WriteLine($"{operation} demands one argument, REMOTE or LOCAL");
                        Console.SetOut(TextWriter.Null);
                        return;
                    }
                    await LoadModInfoAsync();


                    Console.WriteLine($"{operation} not implemented yet");
                    break;
            }
            Console.SetOut(TextWriter.Null);
            SteamClient.Shutdown();
        }

        static async System.Threading.Tasks.Task LoadModInfoAsync()
        {
            workshopItem = (Steamworks.Ugc.Item)await Steamworks.Ugc.Item.GetAsync(workshopId);
        }

        static async System.Threading.Tasks.Task SetModDescriptionAsync(string description)
        {
            var result = await new Steamworks.Ugc.Editor(workshopId).WithDescription(description).SubmitAsync();

            if (result.Success)
            {
                Console.WriteLine($"Description of {workshopItem.Title} updated");
            }
            else
            {
                Console.WriteLine($"Failed to update description of {workshopItem.Title}");
            }
        }

    }
}
