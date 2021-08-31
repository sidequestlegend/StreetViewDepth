using DigitalRuby.Tween;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class WaypointNeighbour : Neighbour
{
    public Transform wayPoint;
    public Vector3 position;
}
public class PlotNeighbors : MonoBehaviour
{
    PhotoSphere PhotoSphere;
    public GameObject PhotoSpherePrefab;
    private List<WaypointNeighbour> Neighbours = new List<WaypointNeighbour>();
    private List<WaypointNeighbour> Directions = new List<WaypointNeighbour>();
    public GameObject WaypointPrefab;

    [HideInInspector]
    public Transform currentWaypoint;

    public Transform Cockpit;

    public Transform LoadingTrigger;

    private bool IsLoading;

    private float ElevationStartOffset;
    // Start is called before the first frame update
    void Start()
    {
        PhotoSphere = GameObject.Find("PhotoSphere").GetComponent<PhotoSphere>();
        PhotoSphere.Initialise();
        PhotoSphere.LoadCallback = () => {
            ElevationStartOffset = PhotoSphere.MetaData.Elavation;
            GPSEncoder.SetLocalOrigin(new Vector2(PhotoSphere.MetaData.Latitude, PhotoSphere.MetaData.Longitude));
            PhotoSphereLoaded();
        };
    }

    void PhotoSphereLoaded() {
        for (int j = 0; j < PhotoSphere.MetaData.Neighbours.Count; j++) {
            Neighbours.Add(new WaypointNeighbour() {
                LatLng = PhotoSphere.MetaData.Neighbours[j].LatLng,
                Panoid = PhotoSphere.MetaData.Neighbours[j].Panoid
            });
        }
        var min_angle = 99999999999f;
        var i = 0;
        foreach (WaypointNeighbour n in Neighbours) {
            var pos = GPSEncoder.GPSToUCS((float)n.LatLng[0], (float)n.LatLng[1]);
            //pos.y = PhotoSphere.MetaData.Elavation - ElevationStartOffset;
            n.position = pos; // new Vector3((float)(pos[0] - StartPoint[0]), 0, (float)(pos[1] - StartPoint[1]));
            var exists = false;
            var direction = (n.position - LoadingTrigger.position).normalized;
            foreach (WaypointNeighbour _n in Directions) {
                var angle = direction.z;
                var angle2 = _n.wayPoint.forward.z;
                var angle3 = direction.x;
                var angle4 = _n.wayPoint.forward.x;
                if (Mathf.Abs(angle - angle2) < 0.1f && Mathf.Abs(angle3 - angle4) < 0.1f) {
                    exists = true;
                }
                if (Vector3.Distance(LoadingTrigger.position, n.position) > 20) {
                    exists = true;
                }
            }
            var start = Instantiate(WaypointPrefab);
            start.name = n.Panoid;
            start.transform.SetParent(transform);
            start.transform.localPosition = n.position;
            start.transform.forward = direction;
            start.SetActive(true);
            n.wayPoint = start.transform;
            var currentAngle = direction - LoadingTrigger.forward;
            if (currentAngle.magnitude < min_angle) {
                min_angle = currentAngle.magnitude;
                currentWaypoint = n.wayPoint;
            }
            if (!exists) {
                Directions.Add(n);
            } else {
                 start.transform.localScale = Vector3.zero;
            }
            i++;
        }
    }

    void Update() {
        if (IsLoading) {
            return;
        }
        var min_angle = 99999999999f;
        string nearestPano = PhotoSphere.Panoid;
        float shortestDistance = Vector3.Distance(PhotoSphere.transform.position, LoadingTrigger.position);
        Vector3 newPosition = PhotoSphere.transform.position;
        foreach (WaypointNeighbour n in Directions) {
            var direction = n.wayPoint.forward;
            var angle = direction.z;
            var currentAngle = direction - Cockpit.forward;
            if (currentAngle.magnitude < min_angle) {
                min_angle = currentAngle.magnitude;
                currentWaypoint = n.wayPoint;
            }
            var distance = Vector3.Distance(n.position, LoadingTrigger.position);
            if (distance < shortestDistance) {
                shortestDistance = distance;
                nearestPano = n.Panoid;
                newPosition = n.position;
            }
        }
        if (nearestPano != PhotoSphere.Panoid) {
            IsLoading = true;
            var newPhotoSphere = Instantiate(PhotoSpherePrefab).GetComponent<PhotoSphere>();
            newPhotoSphere.GetComponent<MeshRenderer>().sharedMaterial = new Material(newPhotoSphere.GetComponent<MeshRenderer>().sharedMaterial);
            newPhotoSphere.GetComponent<MeshRenderer>().sharedMaterial.SetFloat("_Opacity", 0);
            newPhotoSphere.Panoid = nearestPano;
            newPhotoSphere.Initialise();
            newPhotoSphere.LoadCallback = () => {

                // newPosition.y = newPhotoSphere.MetaData.Elavation - ElevationStartOffset;
                newPhotoSphere.transform.position = newPosition;
               // newPhotoSphere.transform.Rotate(currentWaypoint.eulerAngles, Space.World);
                var old = PhotoSphere;
                PhotoSphere = newPhotoSphere;
                var material = old.GetComponent<MeshRenderer>().sharedMaterial;
                var newMaterial = newPhotoSphere.GetComponent<MeshRenderer>().sharedMaterial;
                material.SetFloat("_ZWrite", 0);
                newMaterial.SetFloat("_ZWrite", 0);
                ClearWaypoints();
                PhotoSphereLoaded();
                TweenFactory.Tween(null, 0f, 1f, 0.35f, TweenScaleFunctions.CubicEaseOut, (t2) => {
                    newMaterial.SetFloat("_Opacity", t2.CurrentValue);
                }, (t2) => {
                    TweenFactory.Tween(null, 1f, 0f, 0.35f, TweenScaleFunctions.CubicEaseOut, (_t2) => {
                        material.SetFloat("_Opacity", _t2.CurrentValue);
                    }, (_t2) => {
                        newMaterial.SetFloat("_ZWrite", 1);
                        Destroy(old.gameObject);
                        IsLoading = false;
                        Resources.UnloadUnusedAssets();

                    });
                });
            };
        }
    }

    public void ClearWaypoints() {
        Directions.Clear();
        Neighbours.Clear();
        foreach (Transform child in transform) {
            Destroy(child.gameObject);
        }
    }
}
