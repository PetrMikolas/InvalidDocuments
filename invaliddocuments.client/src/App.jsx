import { useState } from 'react';
import './App.css';
import LoadingMessage from './components/LoadingMessage';
import ErrorMessage from './components/ErrorMessage/ErrorMessage';
import InvalidDocumentDetails from './components/InvalidDocumentDetails/InvalidDocumentDetails';

function App() {
    const [documentValidationResult, setDocumentValidationResult] = useState();
    const [documentNumber, setDocumentNumber] = useState("");
    const [loading, setLoading] = useState(false);
    const [errorMessage, setErrorMessage] = useState("");

    const handleButtonClick = async () => {
        setLoading(true);
        setErrorMessage("");
        setDocumentValidationResult(undefined);

        try {
            const response = await fetch(`validations?number=${documentNumber}`);
            const data = await response.json();

            if (response.status === 400) {
                setErrorMessage(data);
            } else if (response.status === 500) {
                setErrorMessage("Neočekávaná chyba. O problému víme a pracujeme na nápravě.");
            } else {
                setDocumentValidationResult(data);
                setDocumentNumber("");
            }
        } catch (error) {
            console.error("Error:", error);
            setErrorMessage("Nastala chyba při zpracování požadavku. Zkuste to prosím znovu.");
        } finally {
            setLoading(false);
        }
    };

    const getValidationResult = () => {
        if (loading) {
            return <LoadingMessage />;
        } else if (documentValidationResult === undefined && errorMessage === "") {
            return;
        } else if (errorMessage !== "") {
            return <ErrorMessage message={errorMessage} />;
        } else if (documentValidationResult.error !== "") {
            return <ErrorMessage message={documentValidationResult.error} />;
        } else if (documentValidationResult.isRegistered) {
            return <InvalidDocumentDetails details={documentValidationResult} />;
        } else {
            return <p>Doklad s číslem <b>{documentValidationResult.number} nebyl nalezen</b> v databázi neplatných dokladů.</p>;
        }
    };

    return (
        <div>
            <h2 id="tabelLabel">Naplatné doklady</h2>
            <p style={{ marginBottom: '22px' }}>Vyhledávání dokladu v databázi neplatných dokladů MVČR</p>
            <div style={{ textAlign: 'left' }}>
                <div style={{ paddingLeft: '3px' }}>Databáze neplatných dokladů obsahuje údaje pouze o:</div>
                <ul style={{ marginTop: '8px', paddingLeft: '20px' }}>
                    <li>občanských průkazech evidovaných jako ztracené nebo odcizené</li>
                    <li>cestovních pasech evidovaných jako ztracené nebo odcizené</li>
                    <li>zbrojních průkazech a licencích evidovaných jako ztracené nebo odcizené</li>
                </ul>
            </div>
            <p style={{ marginTop: '22px' }}>Zadejte číslo dokladu (pouze číslice a písmena bez diakritiky)</p>
            <input type="text" maxLength="10" value={documentNumber} onChange={(e) => setDocumentNumber(e.target.value)} placeholder="Číslo dokladu" />
            <button onClick={handleButtonClick}>Vyhledat</button>
            {getValidationResult()}
        </div>
    );
}

export default App;