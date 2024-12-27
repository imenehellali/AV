using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ParticipantRow 
{                                                       //PUW
    public string Column1; // String
    public float Column2;  // Float
                                                        //LS
    public string Column3; // String
    public string Column4; // Float or String
                                                        //GB
    public string Column5; // String
    public float Column6;  // Float
                                                        //TM
    public string Column7; // String
    public string Column8;  // Float or string 
                                                        //Game
    public string Column9; // String
    public float Column10;  // Float

    public ParticipantRow(
        string col1, float col2, string col3, string col4,
        string col5, float col6, string col7, string col8, string col9, float col10)
    {
        Column1 = col1;
        Column2 = col2;
        Column3 = col3;
        Column4 = col4;
        Column5 = col5;
        Column6 = col6;
        Column7 = col7;
        Column8 = col8;
        Column9 = col9;
        Column10 = col10;
    }
    public string printAll()
    {
        return $"{Column1} :   {Column2} | {Column3} :  {Column4} |  {Column5} :   {Column6} |  {Column7} : {Column8} |  {Column9} : {Column10}";
    }
}
