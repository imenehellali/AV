using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ParticipantRow 
{
    public string Column1; // String
    public float Column2;  // Float

    public string Column3; // String
    public object Column4; // Float or String

    public string Column5; // String
    public float Column6;  // Float

    public string Column7; // String
    public float Column8;  // Float

    public string Column9; // String
    public float Column10;  // Float

    public ParticipantRow(
        string col1, float col2, string col3, object col4,
        string col5, float col6, string col7, float col8, string col9, float col10)
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
}
