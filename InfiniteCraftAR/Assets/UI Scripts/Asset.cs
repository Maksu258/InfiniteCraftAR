using UnityEngine;
public class Asset
{
    public string name;
    public string link;
    public bool permanent;
    public GameObject model;

    public Asset(string name, string link, bool permanent, GameObject model)
    {
        this.name = name;
        this.link = link;
        this.permanent = permanent;
        this.model = model;
    }
}
