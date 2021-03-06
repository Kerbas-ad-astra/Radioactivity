using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Radioactivity.UI
{
  public class UISourceWindow
  {
    public RadioactiveSource Source {
        get { return source; }
    }

    bool showSourceInfo = false;
    bool showSinkInfo = false;
    bool showWindow = false;
    bool showRays = false;

    int windowID;

    Vector2 iconDims = new Vector2(32f, 32f);
    Vector2 infoBarDims = new Vector2(16,16);
    Vector2 windowDims = new Vector2(150f,20f);

    Vector3 worldPosition;
    Vector3 screenPosition;
    Rect windowPosition;
    RadioactiveSource source;

    Rect atlasIconRect;

    Texture atlas;
    GUIStyle windowStyle;
    GUIStyle groupStyle;
    GUIStyle buttonStyle;
    GUIStyle textHeaderStyle;
    GUIStyle textDescriptorStyle;

    public UISourceWindow(RadioactiveSource src, System.Random random, Texture iconAtlas)
    {
      source = src;
      atlas = iconAtlas;
      windowID = random.Next();
      // Set up screen position
      screenPosition = Camera.main.WorldToScreenPoint(source.part.transform.position);
      windowPosition = new Rect(screenPosition.x+50f, Screen.height-screenPosition.y+windowDims.y/2f, windowDims.x, windowDims.y);
      GetStyles();

      if (source.IconID == 0)
        atlasIconRect = new Rect(0f,0.5f,0.5f,0.5f);
      if (source.IconID == 1)
        atlasIconRect = new Rect(0.5f,0.5f,0.5f,0.5f);
      if (source.IconID == 2)
        atlasIconRect = new Rect(0f,0.0f,0.5f,0.5f);
      if (source.IconID == 3)
        atlasIconRect = new Rect(0.5f,0.0f,0.5f,0.5f);
    }

    internal void GetStyles()
    {
      windowStyle = new GUIStyle(HighLogic.Skin.window);
      windowStyle.fontSize = 10;
      windowStyle.normal.background = null;
      windowStyle.padding = new RectOffset(0,0,0,0);

      groupStyle = new GUIStyle(HighLogic.Skin.textArea);
      groupStyle.normal.background = HighLogic.Skin.window.normal.background;
      groupStyle.padding = new RectOffset(0, 0, 0, 0);

      textHeaderStyle = new GUIStyle(HighLogic.Skin.label);
      textHeaderStyle.normal.textColor = Color.white;
      textHeaderStyle.fontSize = 10;
      textHeaderStyle.stretchWidth = true;
      textHeaderStyle.alignment = TextAnchor.UpperLeft;
      textHeaderStyle.padding = new RectOffset(0,0,0,0);
      textDescriptorStyle = new GUIStyle(HighLogic.Skin.label);
      textDescriptorStyle.alignment = TextAnchor.UpperRight;
      textDescriptorStyle.stretchWidth = true;
      textDescriptorStyle.fontSize = 10;
      textDescriptorStyle.padding = new RectOffset(0, 0, 0, 0);
      buttonStyle = new GUIStyle(HighLogic.Skin.button);
      buttonStyle.fontSize = 8;
      buttonStyle.padding = new RectOffset(0, 0, 0, 0);
        
    }

    public void UpdatePositions()
    {
      screenPosition = Camera.main.WorldToScreenPoint(source.EmitterTransform.position);
      windowPosition = new Rect(screenPosition.x + iconDims.x/2+5f, Screen.height - screenPosition.y + iconDims.y / 2f, windowDims.x, windowDims.y);
    }

    public void Draw()
    {
        if (showWindow)
            windowPosition = GUILayout.Window(windowID, windowPosition, DrawWindow, "", windowStyle, GUILayout.MinHeight(20), GUILayout.ExpandHeight(true));
        if (screenPosition.z > 0f)
            DrawButton();
    }
    internal void DrawButton()
    {
        Rect buttonRect = new Rect(screenPosition.x - iconDims.x / 2f, Screen.height - screenPosition.y - iconDims.y / 2f, iconDims.x, iconDims.y);
        Rect labelRect = new Rect(buttonRect.xMax + 5f, buttonRect.yMin+buttonRect.height/2-infoBarDims.y/2f, 90f, infoBarDims.y);

        GUI.DrawTextureWithTexCoords(buttonRect, atlas, atlasIconRect);
        GUILayout.BeginArea(labelRect, groupStyle);
        GUILayout.BeginHorizontal();
        GUILayout.Label(String.Format("{0}Sv/s", Utils.ToSI(source.CurrentEmission, "F2")), textDescriptorStyle, GUILayout.MinWidth(60f));

        if (GUILayout.Button("...", buttonStyle, GUILayout.Width(12), GUILayout.Height(12)))
        {
            showSourceInfo = !showSourceInfo;
          if (showSourceInfo && !showWindow)
            showWindow = true;
          if (!showSinkInfo && !showSourceInfo)
              showWindow = false;
        }

        GUILayout.EndHorizontal();
        GUILayout.EndArea();
    }
    internal void DrawWindow(int WindowID)
    {
        //if (DrawSinkDetails)
        //  DrawSinkDetails();
        if (showSourceInfo)
          DrawSourceDetails();
    }

    internal void DrawSourceDetails()
    {
      GUILayout.BeginVertical(groupStyle);

      foreach (var kvp in source.GetEmitterDetails())
      {
          GUILayout.Space(2f);
          GUILayout.BeginHorizontal();
          GUILayout.Label(kvp.Key, textHeaderStyle);
          GUILayout.Label(kvp.Value, textDescriptorStyle);
          GUILayout.EndHorizontal();
      }
      GUILayout.EndVertical();
    }
    internal void DrawLinks()
    {
        GUILayout.BeginVertical();
        List<RadiationLink> assocLinks = source.GetAssociatedLinks();
        for (int i = 0; i < assocLinks.Count; i++)
        {
            DrawLink(assocLinks[i]);
        }
        GUILayout.EndVertical();
    }

    void DrawLink(RadiationLink lnk)
    {
        GUILayout.BeginVertical();
        GUILayout.BeginHorizontal(groupStyle);
        GUILayout.BeginVertical();
        GUILayout.Label("<b>" +lnk.source.SourceID + "->" + lnk.sink.SinkID + "</b>", textHeaderStyle);
        GUILayout.Label("I: " + lnk.fluxEndScale.ToString(), textDescriptorStyle);
        GUILayout.EndVertical();
        GUILayout.BeginVertical();
        GUILayout.Label("nZones: " + lnk.ZoneCount.ToString(), textDescriptorStyle);
        GUILayout.Label("nOcclu: " + lnk.OccluderCount.ToString(),textDescriptorStyle);
        GUILayout.EndVertical();
        DrawPathDetails(lnk);
        GUILayout.EndHorizontal();

        GUILayout.EndVertical();
    }
    private void DrawPathDetails(RadiationLink lnk)
    {
        GUILayout.BeginVertical();
        int n = 1;
        foreach (AttenuationZone z in lnk.Path)
        {
            GUILayout.Label(n.ToString() + ". " + z.ToString(), textDescriptorStyle);
            n++;
        }
        GUILayout.EndVertical();
    }
  }

}
