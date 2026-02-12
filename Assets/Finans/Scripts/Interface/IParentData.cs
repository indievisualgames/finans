using System.Collections;
using System.Collections.Generic;
using Firebase.Firestore;
using UnityEngine;

[FirestoreData]
public class FirestoreParentData
{
    [FirestoreProperty]
    public string Firstname { get; set; }
    [FirestoreProperty]
    public string Lastname { get; set; }
    [FirestoreProperty]
    public string Email { get; set; }
    [FirestoreProperty]
    public string Contrycode { get; set; }
    [FirestoreProperty]
    public string Phone { get; set; }
    [FirestoreProperty]
    public string Language { get; set; }
    [FirestoreProperty]
    public string City { get; set; }
    [FirestoreProperty]
    public string Province { get; set; }
    [FirestoreProperty]
    public string Country { get; set; }
    public string Pincode { get; set; }
    [FirestoreProperty]
    public string Kids { get; set; }
    [FirestoreProperty]
    public string Kidsid { get; set; }











}
