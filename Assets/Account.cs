using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Account
{
    public string address { get; set; }
    public string private_key { get; set; }

    public class Account2
    {
        public string your_address { get; set; }
        public string your_private_key { get; set; }
    }

    public class isValidAccount
    {
        public bool message { get; set; }
    }

    public class Address
    {
        public string address { get; set; }
    }

    public class Transaction
    {
        public string sender { get; set; }
        public string receiver { get; set; }
        public string amount { get; set; }
    }


    public class GetMessage
    {
        public string address { get; set; }
        public string other_address { get; set; }
    }

    public class Messages
    {
        public string[] receiver_message { get; set; }
        public string[] sender_message { get; set; }


    }

    public class MineBlock
    {
        public string message { get; set; }
        public int index { get; set; }
        public System.DateTime timestamp { get; set; }
        public int proof { get; set; }
        public string previous_hash { get; set; }
        public List<Transaction> transactions { get; set; }
    }


    public class Balance
    {
        public float balance { get; set; }
    }

    public class AddressList
    {
        public List<string> addresslist { get; set; }
    }



}
