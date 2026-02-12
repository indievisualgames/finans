using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Message
{
    public static string EmptyPin = "Please enter your secret code!";
    public static string IncorrectPinError = "Oops! That's the wrong secret code.";

    public static string ChildSwitchAccountNotFound = "We couldn't find another account to switch to. You're already logged in!";

    public static string ChildAccountNotFound = "We can't find your account. Please try again.";

    public static string AuthenticationInternalError = "Oops! Something went wrong inside. We can't log you in right now.";

    public static string AuthenticationError = "Oops! We couldn't log you in due to an error.";

    public static string DBAuthenticationError = "Oops! We couldn't log you in due to a database error.";

    public static string AuthenticationFaulted = "Oops! We couldn't log you in. Something seems wrong.";

    public static string AuthenticationCanceled = "Oops! Your login was canceled.";

    public static string ChildAuthenticationLabel = "Kid's Secret Code";
    public static string SwitchChildAuthenticationLabel = "Switch Kid";
    public static string NameEmpty = "Don't forget to write your name!";
    public static string ShortName = "Oops! Your name is too short.";

    public static string FirstnameEmpty = "We need your first name!";
    public static string ShortFirstname = "Oops! Your first name is too short.";
    public static string LasttnameEmpty = "We need your last name!";
    public static string ShortLastname = "Oops! Your last name is too short.";
    public static string AgeEmpty = "Please tell us how old you are!";
    public static string FormFillEmptyPin = "Enter your secret code here!";
    public static string PinCharCheck = "Make sure your secret code is 4 digits!";
    public static string FormFillEmptyPinhint = "Set a hint for your secret code!";
    public static string MBNoInternetHeadline = "No Internet Connection";
    public static string MBNoInternetMessage = "Please check your internet connection.";
    public static string MBActionButtonText = "Try Again";
    public static string MBSecondaryButtonText = "Close";
    public static string MBTeritaryButtonText = "Waiting for connection..";
    public static string NoInternetErrorText = "Please check your internet connection.";
    public static string CalulatorNumberClicked = "It is number";
    public static string CalulatorOperatorClicked = "It is operator";
    public static string InvalidFBToken = "No valid Facebook access token available yet";
    public static string FailedFCFromFT = "Failed to create Firebase credential from Facebook token";
    public static string FirebaseCheckInstanceNull = "FirebaseAuthenticationCheck or auth instance is null";
    public static string FirebaseAuthFailed = "Firebase authentication failed";
    public static string FirebaseNullUser = "Firebase returned null user after successful authentication";
    public static string InvalidID = "User id is invalid";
    public static string SignInWithATFailed = "FirebaseSignIn from FB accessToken failed";
    public static string FBAuthenticationFailed = "Facebook authentication failed";
    public static string FBAuthenticationTimeout = "Facebook login timeout. Prompting retry.";

    public static string FirstPartComplete = "Great job! You've completed the first part of this level. Let's see how much you remember in a quick quiz!";

    public static string FirstPartCompleteOnLoad = "Welcome back! Since you've already completed the first part of this level, test your understanding by completing the quiz.";
    public static string CollectTriviaPassOnLoad(string level)
    {
        return $@"Great job finishing the first part! 
The quiz awaits—but you'll need a Trivia Pass first.
Head to Penguin Game level {level}  to earn it and unlock the challenge!";
    }

    public static string CollectTriviaPassOnMarked(string level)
    {
        return $"Collect Trivia Pass From Penguin Game Level {level}";
    }
}
