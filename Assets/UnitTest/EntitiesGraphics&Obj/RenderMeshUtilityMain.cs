using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RenderMeshUtilityMain :MonoBehaviour
{
    public RenderMeshUtilityDraw renderMeshUtilityDraw;

    public Button increaseBtn;
    public Button reduceBtn;
    public Text totalCountTxt;
    
    public Toggle useGameobjctToggle;
    public Toggle usedifferentMaterialToggle;
    public Toggle usedifferentMeshToggle;


    private void Start()
    {
        InitUI();
        renderMeshUtilityDraw.ReStartDraw();
        RefreshInfo();
    }

    private void InitUI()
    {
        useGameobjctToggle.isOn = renderMeshUtilityDraw.m_useGameobjct;
        usedifferentMaterialToggle.isOn = renderMeshUtilityDraw.m_differentMaterial;
        usedifferentMeshToggle.isOn = renderMeshUtilityDraw.m_differentMesh;
        
        
        increaseBtn.onClick.AddListener(() => 
        {
            Debug.Log("AddDrawNumber");
            
            renderMeshUtilityDraw.AddDrawNumber();
            renderMeshUtilityDraw.ReStartDraw();
            RefreshInfo();
        });

        reduceBtn.onClick.AddListener(() =>
        {
            Debug.Log("DecressDrawNumber");
    
            renderMeshUtilityDraw.DecressDrawNumber();
            renderMeshUtilityDraw.ReStartDraw();
            RefreshInfo();
        });
        
        useGameobjctToggle.onValueChanged.AddListener((bool isOn) =>
        {
            Debug.Log("m_useGameobjct");
            renderMeshUtilityDraw.m_useGameobjct = isOn;
            renderMeshUtilityDraw.ReStartDraw();
        });
        usedifferentMaterialToggle.onValueChanged.AddListener((bool isOn) =>
        {
            Debug.Log("m_differentMaterial");
            renderMeshUtilityDraw.m_differentMaterial = isOn;
            renderMeshUtilityDraw.ReStartDraw();
        });
        usedifferentMeshToggle.onValueChanged.AddListener((bool isOn) =>
        {
            Debug.Log("m_differentMesh");
            renderMeshUtilityDraw.m_differentMesh = isOn;
            renderMeshUtilityDraw.ReStartDraw();
        });
    }

    private void RefreshInfo()
    {
        totalCountTxt.text = (renderMeshUtilityDraw.m_h*renderMeshUtilityDraw.m_w).ToString();
    }
    
}