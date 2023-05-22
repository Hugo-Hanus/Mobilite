// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// ROLE CHOISIT
    let selectRole = document.getElementById("role_select");
    let divForm = document.getElementById("roleComplete");
    //let roleChoose= selectRole.options[selectRole.selectedIndex].text;
if(selectRole!=null) {
    selectRole.addEventListener("change", event => {
        let valueChoose = selectRole.value;
        let added = document.getElementById("roleShow");
        let divRole = document.createElement("div");
        divRole.id = "roleShow";
        if (added !== null) {
            added.remove();
        }
        if (valueChoose === 'dispatcher') {
            divRole.innerHTML = `<label class="form-check-label"> Choissisez votre diplome de plus haut niveau</label>
                            <div><input class="form-check-input" name="divName" type="radio" value="CESS" id="diplome"><label class="form-check-label ms-3" for="diplome">CESS</label></div>
                            <div><input class="form-check-input" name="divName" type="radio" value="Bachelier" id="diplome"><label class="form-check-label ms-3" for="diplome">Bachelier</label></div>
                             <div><input class="form-check-input" name="divName" type="radio" value="Licencier" id="diplome"><label class="form-check-label ms-3" for="diplome">Licencier</label></div>`;
            divForm.appendChild(divRole);
        } else if (valueChoose === 'chauffeur') {
            divRole.innerHTML = `<label class="form-check-label"> Choissisez vos permis</label>
                            <div><input class="form-check-input" name="divPermis" type="checkbox" value="B" id="permisB"><label class="form-check-label ms-3" for="permisB">Permis B</label></div>
                            <div><input class="form-check-input" name="divPermis" type="checkbox" value="C" id="permisC"><label class="form-check-label ms-3" for="permisC">Permis C</label></div>
                             <div><input class="form-check-input" name="divPermis" type="checkbox" value="CE" id="permisCE"><label class="form-check-label ms-3" for="permisCE">Permis CE</label></div>`;
            divForm.appendChild(divRole);
        } else {
        }
    });
}


/// SORT ELEMENT STATISTIQUE
let sortDate = document.getElementById("dateSort");
let sortClient = document.getElementById("clientSort");
let sortChauffeur = document.getElementById("chauffeurSort");