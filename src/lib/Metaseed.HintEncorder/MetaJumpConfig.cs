namespace Metaseed.HintEncorder;

public class MetaJumpConfig
{
    /// <summary>Characters used for building jump codes (lowercase letters by default).</summary>
    public string[] CodeChars { get; set; } =
        "k,j,d,f,l,s,a,h,g,i,o,n,u,r,v,c,w,e,x,m,b,p,q,t,y,z"
            .Split(',').Select(c => c.Trim()).ToArray();

    /// <summary>Additional chars that only appear as single-char decoration codes.</summary>
    public string[] AdditionalSingleCodeChars { get; set; } =
        "J,D,F,L,A,H,G,I,N,R,E,M,B,Q,T,Y,1,2,3,4,5,6,7,8,9,0"
            .Split(',').Select(c => c.Trim()).ToArray();

    /// <summary>
    /// Background colors for codes, indexed by (code.Length - 1).
    /// If code length exceeds array length, the last color is used.
    /// </summary>
    public string[] CodeBackgroundColors { get; set; } =
        ["Yellow", "Blue", "Cyan", "Magenta"];

    public string GetBackgroundColor(int codeLength)
    {
        var index = codeLength - 1;
        if (index < 0) index = 0;
        return index < CodeBackgroundColors.Length
            ? CodeBackgroundColors[index]
            : CodeBackgroundColors[^1];
    }
}
