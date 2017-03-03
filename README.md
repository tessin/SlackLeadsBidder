# Gamification of assigning leads using Slack

## Requirements

* .Net hosting with HTTPS
* Slack
* Mandrill
* MSSQL

## Installation 

* Setup a ConnectionString to a MSSQL database in Web.config. 
* Run Enable-Migrations and Update-Database in Package Manager Console to initiate the database. 
* In Slack create a channel and under *Custom Integrations* add a *Incoming WebHooks* for this channel. Enter the generated webhook url in Web.config. 
* Add these *Slack Commands* and enter tokens in Web.config:
   * /bid POST to https://yourdomain/api/slack/bid
   * /autobid POST to https://yourdomain/api/slack/autobid
   * /bids POST to https://yourdomain/api/slack/bids
   * /balance POST to https://yourdomain/api/slack/balance
* Get a Mandrill API key and setup in Web.config. 
* 
* In the database there's now a *Agents* table. For each agent add a row. 
   * 
   
  ## Usage in Slack
  
   ### /balance  
   Show balance for each agent. (private)
   
   ### /bids 
   Show all bids for current auction. (private)
   
   ### /bid X
   Set your bid to X for current auction. 
   
   ### /bid
   Get your current bid. (private)
   
   ### /autobid X
   Set your autobid to X for future auctions
   
   ### /autobid
   Get your current autobid. (private)
   
   
  
  



