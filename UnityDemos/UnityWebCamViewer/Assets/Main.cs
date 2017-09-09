﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Xml;
using UnityEngine;

public class Main : MonoBehaviour {


    [DllImport("CaptureManagerWebCamViewerPlugin")]
    private static extern IntPtr getCollectionOfSources();

    [DllImport("CaptureManagerWebCamViewerPlugin")]
    private static extern IntPtr startCapture(System.IntPtr aSymboliclink, int aStreamIndex, int aMediaTypeIndex, System.IntPtr texture);

    [DllImport("CaptureManagerWebCamViewerPlugin")]
    private static extern void stopCapture();
	
//    [DllImport("CaptureManagerWebCamViewerPlugin")]
//    private static extern void testNvEncoder();
	
    public RenderTexture btnTexture;

    struct SourceData
	{
		public string mFriendlyName;

        public string mSymbolicLink;

        public XmlNode mSourceNode;
	}

    List<SourceData> mSourceDataList = new List<SourceData>();

    struct MediaTypeData
    {
        public string mWidth;

        public string mHeight;

        public string mSymbolicLink;
    }

    List<MediaTypeData> mMediaTypeDataList = new List<MediaTypeData>();

	void OnGUI() 
    {

        //GUI.Box(new Rect(0, 0, 250, 230), btnTexture);
        //if (GUI.Button(new Rect(400, 400, 150, 30), "NvEncoder"))
        //{
        //    testNvEncoder();
        //}

        if (GUI.Button(new Rect(400, 0, 150, 30), "Stop"))
        {
            mainStopCapture();
        }

        float lTopPosition = 0.0f ;

        foreach (var item in mSourceDataList)
	    {
        if (GUI.Button(new Rect(10, lTopPosition, 150, 30), item.mFriendlyName))
            {
                createMediaTypeBtns(item.mSymbolicLink, item.mSourceNode);
            }

            lTopPosition += 35;
	    }

        lTopPosition = 0.0f;

        foreach (var item in mMediaTypeDataList)
        {
            if (GUI.Button(new Rect(200, lTopPosition, 150, 30), item.mWidth + " x " + item.mHeight))
            {
                var lBSTR = Marshal.StringToBSTR(item.mSymbolicLink);

                mainStartCapture(lBSTR, 0, 0, btnTexture.GetNativeTexturePtr());

                Marshal.FreeBSTR(lBSTR);
            }

            lTopPosition += 35;
        }
         
    }

    void createMediaTypeBtns(string aSymbolicLink, XmlNode aSourceNode)
    {

        var lMediaTypeNodes = aSourceNode.SelectNodes("PresentationDescriptor/StreamDescriptor/MediaTypes/MediaType");

        if (lMediaTypeNodes == null)
            return;

        mMediaTypeDataList.Clear();

        foreach (var item in lMediaTypeNodes)
        {
            XmlNode lMediaTypeNode = item as XmlNode;

            if (lMediaTypeNode != null)
            {
                var lWidthAttr = lMediaTypeNode.SelectSingleNode("MediaTypeItem[@Name='MF_MT_FRAME_SIZE']/Value.ValueParts/ValuePart[1]/@Value");

                var lHeightAttr = lMediaTypeNode.SelectSingleNode("MediaTypeItem[@Name='MF_MT_FRAME_SIZE']/Value.ValueParts/ValuePart[2]/@Value");


                var lMediaTypeData = new MediaTypeData();

                lMediaTypeData.mWidth = lWidthAttr.Value;

                lMediaTypeData.mHeight = lHeightAttr.Value;

                lMediaTypeData.mSymbolicLink = aSymbolicLink;


                mMediaTypeDataList.Add(lMediaTypeData);

            }
        } 

    }

    void mainStartCapture(System.IntPtr aSymbolicLink, int aStreamIndex, int aMediaTypeIndex, System.IntPtr texture)
    {

        startCapture(aSymbolicLink, aStreamIndex, aMediaTypeIndex, texture);
    }

    void mainStopCapture()
    {
        stopCapture();
    }

    void getSourceList()
    {
        var lBSTR = getCollectionOfSources();

        string lxmldoc = Marshal.PtrToStringBSTR(lBSTR);
                
        Marshal.FreeBSTR(lBSTR);
        
        XmlDocument doc = new XmlDocument();
        
        doc.LoadXml(lxmldoc);

        var lSourceNodes = doc.SelectNodes("Sources/Source");

        if (lSourceNodes == null)
            return;

        mSourceDataList.Clear();

        foreach (var item in lSourceNodes)
        {
            XmlNode lSourceNode = item as XmlNode;

            if(lSourceNode != null)
            {
                var lFriendlyNameAttr = lSourceNode.SelectSingleNode("Source.Attributes/Attribute[@Name='MF_DEVSOURCE_ATTRIBUTE_FRIENDLY_NAME']/SingleValue/@Value");

                var lSymbolicLinkAttr = lSourceNode.SelectSingleNode("Source.Attributes/Attribute[@Name='MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_SYMBOLIC_LINK']/SingleValue/@Value");
                
                if(lSymbolicLinkAttr != null)
                {
                    var lSourceData = new SourceData();

                    lSourceData.mFriendlyName = lFriendlyNameAttr.Value;

                    lSourceData.mSymbolicLink = lSymbolicLinkAttr.Value;

                    lSourceData.mSourceNode = lSourceNode;

                    mSourceDataList.Add(lSourceData);

                }

            }
        } 
    }




	// Use this for initialization
	void Start () {

        // Create a texture
        RenderTexture tex = new RenderTexture(800, 600, 0, RenderTextureFormat.ARGB32);
        // Set point filtering just so we can see the pixels clearly
        tex.filterMode = FilterMode.Point;
        // Call Apply() so it's actually uploaded to the GPU
        //tex.Apply();

        tex.Create();

        // Set texture onto our matrial
        GetComponent<Renderer>().material.mainTexture = tex;
        
        btnTexture = tex;
        
        getSourceList();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
