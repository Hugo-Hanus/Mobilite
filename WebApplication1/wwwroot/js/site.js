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
            divRole.innerHTML = `<label class="form-check-label"> Choissisez vos diplomes</label>
                            <div><input class="form-check-input" type="checkbox" value="CESS" id="diplomeCess"><label class="form-check-label ms-3" for="diplomeCess">CESS</label></div>
                            <div><input class="form-check-input" type="checkbox" value="Bachelier" id="diplomeBachelier"><label class="form-check-label ms-3" for="diplomeBachelier">Bachelier</label></div>
                             <div><input class="form-check-input" type="checkbox" value="Licencier" id="diplomeLicencier"><label class="form-check-label ms-3" for="diplomeLicencier">Licencier</label></div>`;
            divForm.appendChild(divRole);
        } else if (valueChoose === 'chauffeur') {
            divRole.innerHTML = `<label class="form-check-label"> Choissisez vos permis</label>
                            <div><input class="form-check-input" type="checkbox" value="B" id="permisB"><label class="form-check-label ms-3" for="permisB">Permis B</label></div>
                            <div><input class="form-check-input" type="checkbox" value="C" id="permisC"><label class="form-check-label ms-3" for="permisC">Permis C</label></div>
                             <div><input class="form-check-input" type="checkbox" value="CE" id="permisCE"><label class="form-check-label ms-3" for="permisCE">Permis CE</label></div>`;
            divForm.appendChild(divRole);
        } else {
        }
    });
}
    /// CHAUFFEUR CAMION

let selectChauffeur = document.getElementById("chauffeurSelect");
let divFormCamion=document.getElementById("camionShow");
if(selectChauffeur!=null) {
    selectChauffeur.addEventListener("change", event => {
        let divCamion = document.createElement("div");
        divCamion.innerHTML = `
    <div class="mb-2" id="formChauffeur">
            <select class="form-select" aria-label="Default select example">
              <option selected>Choissisez un Camion</option>
              <option value="1">UJCHF</option>
              <option value="2">AZRFDVD</option>
              <option value="3">EFGGDG</option>
            </select>
        </div>
         <button type="submit" class="btn btn-primary">Confirmer</button>`;
        divFormCamion.appendChild(divCamion);
    });
}


/// SORT ELEMENT STATISTIQUE
let sortDate = document.getElementById("dateSort");
let sortClient = document.getElementById("clientSort");
let sortChauffeur = document.getElementById("chauffeurSort");