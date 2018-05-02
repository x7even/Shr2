# Shr2
Shr2 is a url shortening api, techstack is C# .net core 2.x, makes use of AzureTables for url index storage.

Solution is actually ready to use as replacement for most previous implementations of google url shortener.

- Accept longurl json param for input
- output json of similar format          
* {
*  "kind": "urlshortener#url",
*  "id": "https://your.domain/fbsS",
*  "longUrl": "https://github.com/x7even/Shr2"
*  }
