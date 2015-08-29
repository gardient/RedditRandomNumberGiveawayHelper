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
        private static string RANDOM_ORG_URI = "https://www.random.org/integers/?num=1&min=1&max={0}&col=1&base=10&format=plain&rnd=new";

        public MainForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //textBox2.Text += string.Format("{0}", Environment.NewLine);
            var reddit = new Reddit();
            Post giveawayPost = null;
            int? randomNumber = null;

            Regex numberPost = new Regex(@"(\d{1,6})");

            try
            {
                textBox2.Text += string.Format("Getting giveaway post...{0}", Environment.NewLine);
                giveawayPost = reddit.GetPost(new Uri(textBox1.Text));
            }
            catch (Exception ex)
            {
                textBox2.Text += string.Format("{1}{0}{2}{0}",
                    Environment.NewLine,
                    "Failed getting giveaway post",
                    "You sure that's the right URI (alsomake sure to get the full uri from the address bar)");
                return;
            }

            textBox2.Text += string.Format("{1}{2}{0}{3}{4}{0}",
                Environment.NewLine,
                "Post title: ",
                giveawayPost.Title,
                "Comment count: ",
                giveawayPost.CommentCount);

            try
            {
                textBox2.Text += string.Format("{1}{0}",
                    Environment.NewLine,
                    "Getting random number from random.org...");
                WebRequest randomDotOrgRequest = WebRequest.Create(string.Format(RANDOM_ORG_URI, decimal.Round(randomMax.Value, 0)));
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
                textBox2.Text += string.Format("{1}{0}{2}{0}",
                    Environment.NewLine,
                    "Failed getting giveaway post",
                    ex);
                return;
            }

            textBox2.Text += string.Format("{1}{2}{0}{3}{4}{0}",
                Environment.NewLine,
                "Random Number: ",
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
                        || (x.Value == randomNumber - i && randomNumber - i > 0))
                    {
                        winningNumKey = x.Key;
                        winningNumVal = x.Value;
                        diff = i;
                        break;
                    }
                }
            }

            Comment winningComment = giveawayPost.Comments.FirstOrDefault(w => w.Shortlink == winningNumKey);

            textBox2.Text += string.Format("{1}{2}{0}{3}{4}{0}{5}{6}{0}{7}{8}{0}",
                Environment.NewLine,
                "Winning comment (link): ",
                winningNumKey,
                "Winning comment (body): ",
                winningComment.Body,
                "Winning comment (commenter): ",
                winningComment.Author,
                "Diff: ",
                diff
                );
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
