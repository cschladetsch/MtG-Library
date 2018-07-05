# MtG Library Helper

[TOC]

This project intends to assist with the automation of generating deck descriptions for **Magic the Gathering** (MtG) trading card game.

## Creating an Inventory

The first thing you want to do is make sure you have an accurate record of the cards that you possess.

This record should be stored in a way that is able to be consumed by other MtG online services liked [TappedOut](http://www.tappedout.com).

Making this inventory is tedious: one must enter each card name and add other details.

The purpose of this project is to help with this task by recognising card names and other details from images taken from the webcam on your computer, or otherwise by images of your cards.

### Importing Cards

The general idea is that you just show your cards to your webcam, one after another, and it recognises the Title text, and other details of each card.

You will be given the opportunity to correct any OCR errors and these will be 'remembered' for later.

This information is then stored in a database that you can use to make decks and side-boards.

## Creating Decks

After you have a full Inventory of all your cards, you can make *decks* which are just a subset of cards in your inventory.

It is again useful to abstract and store this in the project so decks can be modified and then later uploaded to other sites like TappedOut.

I am yet unsure where to place the database. It could be on Azure or AWS or something. Not really important.

## Exporting a Deck

There are a number of simple format for MtG deck descriptions, and it is the intent to support them all.

## Side-boards

These will be managed using the same systems as for decks.

Some cross-referencing will be required to map a side-board to a deck, but this is not deemed to be difficult.

## Google Vision API

The app relies on the [Google Vision](https://cloud.google.com/vision/) API. 

Using this is free for a while, then it costs money. Using it requires a private key.

You will need to make your own Private key and deal with any other Auth issues yourself.

The program will read a file called 'GoogleVisionAPIKey.txt' to retrieve the private key. This file is not part of the git repo.

