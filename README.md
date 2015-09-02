# RedditRandomNumberGiveawayHelper
Random number giveaway helper for reddit.
Originally for [/r/pcmasterrace](https://www.reddit.com/r/pcmasterrace/) subreddit, but you're welcome to use it wherever you want to. [It's Free!](http://img4.wikia.nocookie.net/__cb20150729150144/ssb/images/4/4d/Its_free.png)

## Credit where it's due
Huge thanks to [@SirCmpwn](https://github.com/SirCmpwn) for [RedditSharp](https://github.com/SirCmpwn/RedditSharp)!
Sorry for not forking your repository properly, new to the actual usage of git/GitHub and by now I realy have no idea how to properly link back. So you get this space right here at the beggining.

## Usage
![User interface](https://i.imgur.com/njEr6J7.png)

Just copy the link of the giveway post from your address bar and make sure you have the right max value for your random number.
press the button [Give Me A Random Winner] and the following will happen:

1. It fetches the post, displays it's title and the number of comments on it  
(note that these are the comments directly under the post, and that only these comments will enter the giveaway)
2. It fetches a random number from [Random.org](https://www.random.org/), this is displayed for your enjoyment
3. The last step is getting the winning comment. This is done by the following steps:
  1. First compile the numbers from the comments into a list  
  (currently the first integer in the comment, plan to add options here)
  2. Get the oldest post with the number closest to the random number
  3. Display link, body, and author of winning comment, as well as the difference (for your viewing pleasure)

## Features to come

- Change to new random.org API (yes this uses the old one)
- ~~Add minimum value to random number~~(Done!)
- Add better defined matching if number with which to enter the lottery
  - Number has to be right at the beginning of the comment
  - Dropdown with more options
  - Custom matching with regular expressions
- Filtering comments by date (in case you want to be strict with the deadline)
