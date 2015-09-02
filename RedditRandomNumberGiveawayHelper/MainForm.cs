using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RedditSharp;
using RedditSharp.Things;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using Octokit;

namespace RedditRandomNumberGiveawayHelper
{
    public partial class MainForm : Form
    {
        //make sure to format with max number
        private static string RANDOM_ORG_URI = "https://www.random.org/integers/?num=1&min={0}&max={1}&col=1&base=10&format=plain&rnd=new";

        public MainForm()
        {
            InitializeComponent();
        }

        class DisplayTextbox
        {
            private TextBox txtBx;

            public DisplayTextbox(TextBox x)
            {
                txtBx = x;
            }

            public void Write(params object[] strings)
            {
                txtBx.Text += string.Join(Environment.NewLine, strings) + Environment.NewLine;
            }

            public void Clear()
            {
                txtBx.Clear();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DisplayTextbox displayTextBox = new DisplayTextbox(textBox2);

            displayTextBox.Clear();
            var reddit = new Reddit();

            Post giveawayPost = null;
            int? randomNumber = null;

            Regex numberPost = new Regex(@"(\d{1,6})");

            try
            {
                displayTextBox.Write("Getting giveaway post...");
                giveawayPost = reddit.GetPost(new Uri(textBox1.Text));
            }
            catch (Exception ex)
            {
                displayTextBox.Write("Failed getting giveaway post",
                    "You sure that's the right URI (alsomake sure to get the full uri from the address bar)");
                return;
            }

            displayTextBox.Write("Post title: ",
                giveawayPost.Title,
                "Comment count: ",
                giveawayPost.CommentCount);

            try
            {
                displayTextBox.Write("Getting random number from random.org...");

                WebRequest randomDotOrgRequest = WebRequest.Create(string.Format(RANDOM_ORG_URI, decimal.Round(randomMin.Value, 0), decimal.Round(randomMax.Value, 0)));
                using (WebResponse resp = randomDotOrgRequest.GetResponse())
                {
                    using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
                    {
                        randomNumber = int.Parse(sr.ReadToEnd());
                    }
                }
            }
            catch (Exception ex)
            {
                displayTextBox.Write("Failed getting giveaway post", ex);
                return;
            }

            displayTextBox.Write("Random Number: ",
                randomNumber,
                "Getting winning comment...",
                "This might take a while...");

            Dictionary<string, int> nums = giveawayPost.Comments.Where(c => c.Body != null && numberPost.IsMatch(c.Body)).ToDictionary(k => k.Shortlink, elementSelector: x => int.Parse(numberPost.Match(x.Body).Captures[0].Value));
            string winningNumKey = null;
            int? winningNumVal = null;
            int? diff = null;

            for (int i = 0; (winningNumKey == null && winningNumVal == null) && (randomNumber + i < decimal.Round(randomMax.Value, 0) || randomNumber - i > 0); i++)
            {
                foreach (var x in nums)
                {
                    if ((x.Value == randomNumber + i && randomNumber + i < decimal.Round(randomMax.Value, 0))
                        || (x.Value == randomNumber - i && randomNumber - i > decimal.Round(randomMin.Value, 0)))
                    {
                        winningNumKey = x.Key;
                        winningNumVal = x.Value;
                        diff = i;
                        break;
                    }

                    //stop if we're out of range
                    if (randomNumber + i > decimal.Round(randomMax.Value, 0) && randomNumber - i < decimal.Round(randomMin.Value, 0))
                        break;
                }
            }

            if (!string.IsNullOrEmpty(winningNumKey) && winningNumVal.HasValue)
            {
                Comment winningComment = giveawayPost.Comments.FirstOrDefault(w => w.Shortlink == winningNumKey);

                displayTextBox.Write("Winning comment (link): ",
                    winningNumKey,
                    "Winning comment (body): ",
                    winningComment.Body,
                    "Winning comment (commenter): ",
                    winningComment.Author,
                    "Diff: ",
                    diff
                    );
            }
            else
            {
                displayTextBox.Write("What the heck?? not one post in the range? you sure you have that range right?");
            }
        }

        private async void checkForUpdatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var github = new GitHubClient(new ProductHeaderValue("RedditRandomNumberGiveawayHelper"));
            var releases = await github.Release.GetAll("gardient", "RedditRandomNumberGiveawayHelper");
            var newestRelease = releases.OrderByDescending(x => x.Id).FirstOrDefault();
            var versionRegex = new Regex(@"^v(?<major>\d+)\.(?<minor>\d+)(\.(?<patch>\d+))?$", RegexOptions.IgnoreCase);
            var tagMatch = versionRegex.Match(newestRelease.TagName);
            if (tagMatch.Success)
            {
                var versionMatch = versionRegex.Match(Properties.Settings.Default.Version);
                if (versionMatch.Success)
                {
                    if (int.Parse(versionMatch.Result("${major}")) >= int.Parse(tagMatch.Result("${major}"))
                        && int.Parse(versionMatch.Result("${minor}")) >= int.Parse(tagMatch.Result("${minor}"))
                        && int.Parse(versionMatch.Result("${patch}") == "" ? "0" : versionMatch.Result("${patch}")) >= int.Parse(tagMatch.Result("${patch}") == "" ? "0" : tagMatch.Result("${patch}")))
                    {
                        MessageBox.Show("You have the latest version");
                        return;
                    }
                    else
                    {
                        MessageBox.Show("There is a new verion available");
                        System.Diagnostics.Process.Start(newestRelease.HtmlUrl);
                        return;
                    }
                }
            }
        }
    }
}
