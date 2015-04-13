# RecSys

The RecSys library implements different RecSys algorithms described in research papers including mine. The library is written in C# for the .NET platform (https://github.com/Microsoft/dotnet), and runs on Windows, Linux, and MacOS X.

Implemented Algorithms
----------------------
  - **Matrix Factorization** [[1]](#1): 
    - the standard and popular matrix factorization approach to collaborative filtering.
  - **UserKNN** [[2]](#2): 
    - the standard and popular K-Nearest Neighbors approach to collaborative filtering.
  - **PrefUserKNN** [[3]](#3):
    - UserKNN plus accepting *preference relations* as input to perofrm top-N recommendation.
  - **PrefNMF Matrix Factorization** [[4]](#4):
    - Matrix Factorization plus accepting *preference relations* as input to perofrm top-N recommendation.
  - **Ordinal Random Fields** [[5]](#5):
    - a combination of Markov Random Fields and Ordinal Matrix Factorization techniques.
    - exploits both *Global* and *Local* structures in the rating matrix.
    - exploits *ordinal properties* of ratings.
  - **Preference Random Fields** [[6]](#6):
    - exploits both *Global* and *Local* structures in the rating matrix.
    - takes *preference relations* as input to perofrm top-N recommendation.
  
  
Installation
-------------
Coming soon.



References
----------
  
 1. Koren, Y., Bell, R., & Volinsky, C. (2009). [Matrix factorization techniques for recommender systems](http://dx.doi.org/10.1109/MC.2009.263). Computer, (8), 30-37. [[PDF]](http://www2.research.att.com/~volinsky/papers/ieeecomputer.pdf)
 2. Konstan, J. A., Miller, B. N., Maltz, D., Herlocker, J. L., Gordon, L. R., & Riedl, J. (1997). [GroupLens: applying collaborative filtering to Usenet news](http://dx.doi.org/10.1145/245108.245126). Communications of the ACM, 40(3), 77-87. [[PDF]](https://www.ischool.utexas.edu/~i385q/readings/konstan_1997.pdf)
 3. Brun, A., Hamad, A., Buffet, O., & Boyer, A. (2010, September). [Towards preference relations in recommender systems](http://www.ke.tu-darmstadt.de/events/PL-10/papers/1-Brun.pdf). In Workshop on Preference Learning, European Conference on Machine Learning and Principle and Practice of Knowledge Discovery in Databases (ECML-PKDD 2010). [[PDF]](http://www.ke.tu-darmstadt.de/events/PL-10/papers/1-Brun.pdf)
 4. Desarkar, M. S., Saxena, R., & Sarkar, S. (2012). [Preference relation based matrix factorization for recommender systems](http://dx.doi.org/10.1007/978-3-642-31454-4_6). In User Modeling, Adaptation, and Personalization (pp. 63-75). Springer Berlin Heidelberg. [[PDF]](http://www.researchgate.net/profile/Sudeshna_Sarkar2/publication/241770977_Preference_relation_based_matrix_factorization_for_recommender_systems/links/0deec53606e7ad7334000000.pdf)
 5. S.-W. Liu, T. Tran, G. Li and Y, Jiang. [Ordinal Random Fields for Recommender Systems](http://www.jmlr.org/proceedings/papers/v39/liu14.html). In Proceedings of the Sixth Asian Conference on Machine Learning (ACML 2014), pp. 283–298, 2014. [[PDF]](http://prada-research.net/~truyen/papers/liu2014ordinal.pdf)

  


Copyright & Licensing
---------------------
Coming soon.
