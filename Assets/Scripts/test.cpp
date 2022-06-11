#include<iostream>
#include<cmath>

using namespace std;

int main()
{
    double a;
    cin>>a;

    if(a >= pow(-2,31) && a<= (pow(2,31)-1))
    cout<<"Yes";

    else
    cout<<"No";

    return 0;
}