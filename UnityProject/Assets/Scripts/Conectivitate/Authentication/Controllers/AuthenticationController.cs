﻿using Conectivitate.Authentication.Models;
using Proyecto26;
using System.Collections;
using System.Collections.Generic;
using Proyecto26;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

//Controller pentru toate operatiile legate de autentificare
public class AuthenticationController : MonoBehaviour
{
    //pentru a comunica cu API-ul firebase
    private string authKey = "AIzaSyDIL9FeYzduvqF3jA5h1osoLhJEj5hBmQw";
    private string databaseURL = "https://dungeonsanddragons-ip2020.firebaseio.com/";

    //instanta de utilizator
    User user = new User();

    //un idToken privat care il identifica unic
    private string idToken;

    //inputurile din formulare
    //sign in
    public InputField signInEmailText;
    public InputField signInPasswordText;

    //register
    public InputField registerUsernameText;
    public InputField registerEmailText;
    public InputField registerPasswordText;
    public InputField registerConfirmPasswordText;

    //Activates when the user clicks the RegisterUserButton
    public void RegisterUserButton()
    {
        //verificam validitatea inputurilor
        if (registerEmailText.text != "" && registerUsernameText.text != "" && registerPasswordText.text != "" &&
            registerConfirmPasswordText.text != "" &&
            registerPasswordText.text.Equals(registerConfirmPasswordText.text))
        {
            RegisterUser(registerEmailText.text, registerUsernameText.text, registerPasswordText.text);
            Application.LoadLevel("Start Menu");
        }
    }

    //Populeaza informatiile din user daca se poate crea contul, altfel vom avea o eroare HTTP BAD REQUEST de la serverul firebase
    private void RegisterUser(string email, string username, string password)
    {
        RequestHelper currentRequest = new RequestHelper
        {
            Uri = "https://www.googleapis.com/identitytoolkit/v3/relyingparty/signupNewUser",
            Params = new Dictionary<string, string>
            {
                {"key", authKey}
            },
            Body = new NewUserRegister
            {
                email = email,
                password = password,
                returnSecureToken = true
            },
            EnableDebug = true
        };
        RestClient.Post<AuthResponse>(currentRequest).Then(
            response =>
            {
                Debug.Log("Response:");
                idToken = response.idToken;
                user.localId = response.localId;
                user.userName = username;
                PostToDatabase(user);
            }).Catch(error => { Debug.Log(error); });
    }

    //Posteaza in baza de date informatiile despre utilizatorul primit(username, si pe viitor si alte detalii)
    public void PostToDatabase(User user)
    {
        RestClient.Put(databaseURL + "/" + user.localId + ".json?auth=" + idToken, user);
    }

    //Cand utilizatorul apasa pe login button
    public void LoginUserButton()
    {
        if (signInEmailText.text != "" && signInPasswordText.text != "")
        {
            LoginUser("robert.tabacaru@yahoo.com", signInPasswordText.text);
            Application.LoadLevel("Start Menu");
        }
    }

    //Returneaza datele utilizatorului, sau eroare in caz ca e introdus un mail gresit
    private void LoginUser(string email, string password)
    {
        RequestHelper currentRequest = new RequestHelper
        {
            Uri = "https://www.googleapis.com/identitytoolkit/v3/relyingparty/verifyPassword",
            Params = new Dictionary<string, string>
            {
                {"key", authKey}
            },
            Body = new NewUserRegister
            {
                email = email,
                password = password,
                returnSecureToken = true
            },
            EnableDebug = true
        };
        RestClient.Post<AuthResponse>(currentRequest).Then(
            response =>
            {
                Debug.Log("Response:");
                idToken = response.idToken;
                user.localId = response.localId;

                GetUsernameFromDatabase(user);
            }).Catch(error => { Debug.Log(error); });
    }

    //Pentru a obtine username-ul, posibil pe viitor si alte date, din baza de date firebase
    private void GetUsernameFromDatabase(User user)
    {
        RestClient.Get<User>(databaseURL + "/" + user.localId + ".json?auth=" + idToken).Then(response =>
        {
            user.userName = response.userName;
        });
    }
}