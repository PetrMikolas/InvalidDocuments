/* eslint-disable react-hooks/exhaustive-deps */
import { useEffect, useState } from 'react';
import './App.css';

function App() {
    const [document, setDocument] = useState();
    const [documentNumber, setDocumentNumber] = useState("");
    const [buttonClicked, setButtonClicked] = useState(false);
    const [loading, setLoading] = useState(false);
    const [errorMessage, setErrorMessage] = useState("");

    useEffect(() => {
        if (buttonClicked) {
            setLoading(true);
            populateDocumentData();
        }
    }, [buttonClicked]);

    const handleButtonClick = () => {
        setButtonClicked(true);
        setDocument(undefined);
    };

    const contents = loading
        ? <p><em>Vyhledávání...</em></p>
        : document === undefined
            ? <p style={{ color: 'red' }}>{errorMessage}</p>
            : document.isRegistered
                ? <>
                    <p>Doklad s číslem <b>{document.number} byl nalezen</b> v databázi neplatných dokladů.</p>
                    <div className="table-container">
                        <table className="table table-striped" aria-labelledby="tabelLabel">
                            <thead>
                                <tr>
                                    <th>Číslo dokladu</th>
                                    <th>Série</th>
                                    <th>Typ dokladu</th>
                                    <th>Neplatný od</th>
                                </tr>
                            </thead>
                            <tbody>
                                <tr>
                                    <td>{document.number}</td>
                                    <td>{document.series}</td>
                                    <td>{document.type}</td>
                                    <td>{document.registeredFrom}</td>
                                </tr>
                            </tbody>
                        </table>
                    </div>
                </>
                : document.error !== ""
                    ? <p style={{ color: 'red' }}>{document.error}</p>
                    : <p>Doklad s číslem <b>{document.number} nebyl nalezen</b> v databázi neplatných dokladů.</p>;

    return (
        <div>
            <h2 id="tabelLabel">Naplatné doklady</h2>
            <p>Vyhledávání dokladu v databázi neplatných dokladů MVČR</p>
            <p>Zadejte číslo dokladu (pouze číslice a písmena bez diakritiky)</p>
            <input type="text" maxLength="10" value={documentNumber} onChange={(e) => setDocumentNumber(e.target.value)} placeholder="Číslo dokladu" />
            <button onClick={handleButtonClick}>Vyhledat</button>
            {contents}
        </div>
    );

    async function populateDocumentData() {
        try {
            const response = await fetch(`documents?number=${documentNumber}`);
            const data = await response.json();

            if (response.status === 400) {
                setErrorMessage(data);
            } else if (response.status === 500) {
                setErrorMessage("Neočekávaná chyba. O problému víme a pracujeme na nápravě.");
            } else {
                setDocument(data);
                setDocumentNumber("");
            }
        } catch (error) {
            console.error("Error:", error);
            setErrorMessage("Nastala chyba při zpracování požadavku. Zkuste to prosím znovu.");
        } finally {
            setLoading(false);
            setButtonClicked(false);
        }
    }
}

export default App;
