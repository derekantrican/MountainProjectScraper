﻿using MountainProjectAPI;
using RedditSharp.Things;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;

namespace MountainProjectBot
{
    public static class BotReply
    {
        const string BOTKEYWORDREGEX = @"(?i)!mountain\s*project(.*)";

        public static string GetReplyForCommentBody(string commentBody)
        {
            //Get everything AFTER the keyword, but on the same line
            string queryText = Regex.Match(commentBody, BOTKEYWORDREGEX).Groups[1].Value.Trim();
            if (string.IsNullOrWhiteSpace(queryText))
                return "I didn't understand what you were looking for. Please use the Feedback button below if you think this is a bug";

            SearchParameters parameters = MountainProjectDataSearch.ParseParameters(ref queryText);

            MPObject searchResult = MountainProjectDataSearch.FilterByPopularity(MountainProjectDataSearch.SearchMountainProject(queryText, parameters));
            string replyText = GetFormattedString(searchResult);
            if (string.IsNullOrEmpty(replyText))
            {
                if (parameters != null && !string.IsNullOrEmpty(parameters.SpecificLocation))
                    replyText = $"I could not find anything for \"{queryText}\" in \"{parameters.SpecificLocation}\". Please use the Feedback button below if you think this is a bug";
                else
                    replyText = $"I could not find anything for \"{queryText}\". Please use the Feedback button below if you think this is a bug";
            }

            return replyText;
        }

        public static string GetFormattedString(MPObject inputMountainProjectObject)
        {
            if (inputMountainProjectObject == null)
                return null;

            string result = "I found the following info:" + Markdown.NewLine;

            if (inputMountainProjectObject is Area)
            {
                Area inputArea = inputMountainProjectObject as Area;
                result += $"{Markdown.Bold(inputArea.Name)} [{inputArea.Statistics}]" + Markdown.NewLine;
                result += GetLocationString(inputArea);
                result += GetPopularRoutes(inputArea);

                result += inputArea.URL;
            }
            else if (inputMountainProjectObject is Route)
            {
                Route inputRoute = inputMountainProjectObject as Route;
                result += $"{Markdown.Bold(inputRoute.Name)}" + Markdown.NewLine;
                result += $"Type: {string.Join(", ", inputRoute.Types)}";
                result += $"Grade: {inputRoute.Grade}";

                if (!string.IsNullOrEmpty(inputRoute.AdditionalInfo))
                    result += ", " + inputRoute.AdditionalInfo;

                result += Markdown.NewLine;
                result += $"Rating: {inputRoute.Rating}/4" + Markdown.NewLine;
                result += GetLocationString(inputRoute);

                result += inputRoute.URL;
            }

            return result;
        }

        public static string GetLocationString(MPObject child)
        {
            MPObject innerParent, outerParent;
            innerParent = null;
            if (child is Route)
                innerParent = MountainProjectDataSearch.GetParent(child, -2); //Get the "second to last" parent https://github.com/derekantrican/MountainProject/issues/12
            else if (child is Area)
                innerParent = MountainProjectDataSearch.GetParent(child, -1); //Get immediate parent

            if (innerParent == null ||  //If "child" is a dest area, the parent will be "All Locations" which won't be in our directory
                innerParent.URL == Utilities.INTERNATIONALURL) //If "child" is an area like "Europe"
                return "";

            outerParent = MountainProjectDataSearch.GetParent(child, 1); //Get state that route/area is in
            if (outerParent.URL == Utilities.INTERNATIONALURL) //If this is international, get the country instead of the state (eg "China")
            {
                if (child.ParentUrls.Count > 3)
                {
                    if (child.ParentUrls.Contains(Utilities.AUSTRALIAURL)) //Australia is both a continent and a country so it is an exception
                        outerParent = MountainProjectDataSearch.GetParent(child, 2);
                    else
                        outerParent = MountainProjectDataSearch.GetParent(child, 3);
                }
                else
                    return ""; //Return a blank string if we are in an area like "China" (so we don't return a string like "China is located in Asia")
            }

            string locationString = $"Located in {Markdown.Link(innerParent.Name, innerParent.URL)}";
            if (outerParent != null && outerParent.URL != innerParent.URL)
                locationString += $", {Markdown.Link(outerParent.Name, outerParent.URL)}";

            locationString += Markdown.NewLine;

            return locationString;
        }

        public static string GetPopularRoutes(Area area)
        {
            string result = "Popular routes:\n";

            if (area.PopularRouteUrls.Count == 0)
                return "";

            foreach (string url in area.PopularRouteUrls)
            {
                List<MPObject> itemsToSearch = new List<MPObject>();
                itemsToSearch.AddRange(area.SubAreas);
                itemsToSearch.AddRange(area.Routes);

                Route popularRoute = MountainProjectDataSearch.GetItemWithMatchingUrl(url, itemsToSearch) as Route;
                result += $"\n- {Markdown.Link(popularRoute.Name, popularRoute.URL)} [{popularRoute.Grade}";
                if (!string.IsNullOrEmpty(popularRoute.AdditionalInfo))
                    result += $", {popularRoute.AdditionalInfo}";

                result += "]";
            }

            result += Markdown.NewLine;

            return result;
        }
    }
}
